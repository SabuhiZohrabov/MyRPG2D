using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DatabaseManager
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DatabaseManager();
            }
            return _instance;
        }
    }

    private SQLiteConnection _db;
    private bool _isInitialized = false;
    private readonly object _lockObject = new object();

    private string GetDbFileName()
    {
        return StoryManager.SelectedStoryId + "Gamedata.db";
    }

    private DatabaseManager()
    {
        InitializeDatabase();

#if UNITY_EDITOR
        // listen to editor play mode changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void InitializeDatabase()
    {
        if (!Application.isPlaying || _isInitialized) return;

        lock (_lockObject)
        {
            if (_isInitialized) return;

            try
            {
                string dbPath = Path.Combine(Application.persistentDataPath, GetDbFileName());
                _db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, true);
                Debug.Log($"[SQLite] DB initialized at: {dbPath}");

                // create tables
                _db.CreateTable<PlayerStatsModel>();
                _db.CreateTable<PlayerSkillModel>();
                _db.CreateTable<InventoryItemModel>();
                _db.CreateTable<AdventureProgressModel>();
                _db.CreateTable<ConditionModel>();

                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] Database initialization failed: {e.Message}");
            }
        }
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("[SQLite] Play mode exiting, closing database connection...");
            if (_instance != null)
            {
                _instance.CloseConnection();
                _instance = null;
            }
        }
    }
#endif

    // -----------------------
    // PlayerStats operations
    // -----------------------

    #region player
    public PlayerStatsModel GetPlayerStats()
    {
        if (_db == null) return new PlayerStatsModel();

        lock (_lockObject)
        {
            try
            {
                var existing = _db.Table<PlayerStatsModel>().FirstOrDefault();
                if (existing != null)
                    return existing;

                var newStats = new PlayerStatsModel();
                _db.Insert(newStats);
                return newStats;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetPlayerStats failed: {e.Message}");
                return new PlayerStatsModel();
            }
        }
    }

    public void SavePlayerStats(PlayerStatsModel stats)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    if (stats.Id == 0)
                        _db.Insert(stats);
                    else
                        _db.Update(stats);

                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] SavePlayerStats failed: {e.Message}");
            }
        }
    }

    public void DeleteAllData()
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    _db.DeleteAll<PlayerStatsModel>();
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] DeleteAllData failed: {e.Message}");
            }
        }
    }

    public static void LoadStatsToCharacter(CharacterStats stats)
    {
        var saved = DatabaseManager.Instance.GetPlayerStats();
        stats.Level = saved.Level;
        stats.CurrentXP = saved.CurrentXP;
        stats.XPToNextLevel = saved.XPToNextLevel;
        stats.AvailableAttributePoints = saved.AvailableAttributePoints;
        stats.Strength.Value = saved.Strength;
        stats.Dexterity.Value = saved.Dexterity;
        stats.Intelligence.Value = saved.Intelligence;
        stats.Endurance.Value = saved.Endurance;
        stats.CurrentAdventureId = saved.CurrentAdventureId;
        stats.DatabaseId = saved.Id;
        
        // Load player skills from separate PlayerSkill table
        var skillModels = DatabaseManager.Instance.GetPlayerSkills(saved.Id);
        stats.playerSkills = skillModels.Select(s => s.SkillName).ToList();
    }

    public void SaveToDatabase(CharacterStats stats)
    {
        PlayerStatsModel data = new PlayerStatsModel
        {
            Id = stats.DatabaseId,
            Level = stats.Level,
            CurrentXP = stats.CurrentXP,
            XPToNextLevel = stats.XPToNextLevel,
            AvailableAttributePoints = stats.AvailableAttributePoints,
            Strength = stats.Strength.Value,
            Dexterity = stats.Dexterity.Value,
            Intelligence = stats.Intelligence.Value,
            Endurance = stats.Endurance.Value,
            CurrentAdventureId = stats.CurrentAdventureId ?? "start_adventure"
        };

        DatabaseManager.Instance.SavePlayerStats(data);
        
        // Save player skills to separate PlayerSkill table
        DatabaseManager.Instance.SavePlayerSkills(data.Id, stats.playerSkills);
    }
    #endregion

    // -----------------------
    // PlayerSkill operations
    // -----------------------

    #region playerskills
    public List<PlayerSkillModel> GetPlayerSkills(int playerId)
    {
        if (_db == null) return new List<PlayerSkillModel>();

        lock (_lockObject)
        {
            try
            {
                return _db.Table<PlayerSkillModel>()
                          .Where(s => s.PlayerId == playerId)
                          .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetPlayerSkills failed: {e.Message}");
                return new List<PlayerSkillModel>();
            }
        }
    }

    public void SavePlayerSkills(int playerId, List<string> skills)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    // Delete existing skills for this player
                    var existingSkills = GetPlayerSkills(playerId);
                    foreach (var skill in existingSkills)
                    {
                        _db.Delete(skill);
                    }

                    // Insert new skills
                    foreach (var skillName in skills)
                    {
                        var skillModel = new PlayerSkillModel(playerId, skillName);
                        _db.Insert(skillModel);
                    }

                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] SavePlayerSkills failed: {e.Message}");
            }
        }
    }

    public void AddPlayerSkill(int playerId, string skillName)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                // Check if skill already exists
                var existingSkill = _db.Table<PlayerSkillModel>()
                                      .FirstOrDefault(s => s.PlayerId == playerId && s.SkillName == skillName);

                if (existingSkill == null)
                {
                    var skillModel = new PlayerSkillModel(playerId, skillName);
                    _db.Insert(skillModel);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] AddPlayerSkill failed: {e.Message}");
            }
        }
    }

    public void RemovePlayerSkill(int playerId, string skillName)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                var skillToRemove = _db.Table<PlayerSkillModel>()
                                      .FirstOrDefault(s => s.PlayerId == playerId && s.SkillName == skillName);

                if (skillToRemove != null)
                {
                    _db.Delete(skillToRemove);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] RemovePlayerSkill failed: {e.Message}");
            }
        }
    }
    #endregion

    // -----------------------
    // Inventory operations
    // -----------------------

    #region inventory
    public InventoryItemModel GetInventoryItem(string itemId)
    {
        if (_db == null) return null;

        lock (_lockObject)
        {
            try
            {
                return _db.Table<InventoryItemModel>()
                          .FirstOrDefault(i => i.ItemId == itemId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetInventoryItem failed: {e.Message}");
                return null;
            }
        }
    }

    public void InsertInventoryItem(InventoryItemModel item)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    _db.Insert(item);
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] InsertInventoryItem failed: {e.Message}");
            }
        }
    }

    public void UpdateInventoryItem(InventoryItemModel item)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    _db.Update(item);
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] UpdateInventoryItem failed: {e.Message}");
            }
        }
    }

    public void DeleteInventoryItem(string itemId)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    var existing = GetInventoryItem(itemId);
                    if (existing != null)
                    {
                        _db.Delete(existing);
                    }
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] DeleteInventoryItem failed: {e.Message}");
            }
        }
    }

    public List<InventoryItemModel> GetAllInventoryItems()
    {
        if (_db == null) return new List<InventoryItemModel>();

        lock (_lockObject)
        {
            try
            {
                return _db.Table<InventoryItemModel>().ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetAllInventoryItems failed: {e.Message}");
                return new List<InventoryItemModel>();
            }
        }
    }
    #endregion

    // -----------------------
    // Adventure operations
    // -----------------------

    public AdventureProgressModel GetAdventureProgress()
    {
        if (_db == null) return new AdventureProgressModel { CurrentAdventureId = "start_adventure" };

        lock (_lockObject)
        {
            try
            {
                var existing = _db.Table<AdventureProgressModel>().FirstOrDefault();
                if (existing != null)
                    return existing;

                var progress = new AdventureProgressModel
                {
                    CurrentAdventureId = "start_adventure"
                };
                _db.Insert(progress);
                return progress;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetAdventureProgress failed: {e.Message}");
                return new AdventureProgressModel { CurrentAdventureId = "start_adventure" };
            }
        }
    }

    public void SaveAdventureProgress(string currentAdventureId)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    var progress = GetAdventureProgress();
                    progress.CurrentAdventureId = currentAdventureId;
                    _db.InsertOrReplace(progress);
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] SaveAdventureProgress failed: {e.Message}");
            }
        }
    }

    // -----------------------
    // Condition operations
    // -----------------------

    #region conditions
    public ConditionModel GetCondition(string conditionId)
    {
        if (_db == null) return null;

        lock (_lockObject)
        {
            try
            {
                return _db.Table<ConditionModel>()
                          .FirstOrDefault(c => c.ConditionId == conditionId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetCondition failed: {e.Message}");
                return null;
            }
        }
    }

    public int GetConditionValue(string conditionId)
    {
        var condition = GetCondition(conditionId);
        return condition?.Value ?? 0;
    }

    public void SetConditionValue(string conditionId, int value)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    var existing = GetCondition(conditionId);
                    if (existing != null)
                    {
                        existing.Value = value;
                        _db.Update(existing);
                    }
                    else
                    {
                        var newCondition = new ConditionModel(conditionId, value);
                        _db.Insert(newCondition);
                    }
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] SetConditionValue failed: {e.Message}");
            }
        }
    }

    public void AddValueToCondition(string conditionId, int valueToAdd)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    var existing = GetCondition(conditionId);
                    if (existing != null)
                    {
                        existing.Value += valueToAdd;
                        _db.Update(existing);
                    }
                    else
                    {
                        var newCondition = new ConditionModel(conditionId, valueToAdd);
                        _db.Insert(newCondition);
                    }
                    _db.Commit();
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] AddValueToCondition failed: {e.Message}");
            }
        }
    }

    public List<ConditionModel> GetAllConditions()
    {
        if (_db == null) return new List<ConditionModel>();

        lock (_lockObject)
        {
            try
            {
                return _db.Table<ConditionModel>().ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] GetAllConditions failed: {e.Message}");
                return new List<ConditionModel>();
            }
        }
    }
    #endregion

    // -----------------------
    // General operations
    // -----------------------
    public void ResetGame()
    {
        try
        {
            CloseConnection();

            // Find all database files (files ending with "_gamedata.db")
            string[] dbFiles = Directory.GetFiles(Application.persistentDataPath, "*.db");

            // Delete each database file
            foreach (string dbFilePath in dbFiles)
            {
                if (System.IO.File.Exists(dbFilePath))
                {
                    System.IO.File.Delete(dbFilePath);
                }
            }

            // Reset initialization flag and create new connection
            _isInitialized = false;
            //InitializeDatabase();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SQLite] ResetGame failed: {e.Message}");
        }
    }

    public void CloseConnection()
    {
        lock (_lockObject)
        {
            try
            {
                if (_db != null)
                {
                    _db.Close();
                    _db.Dispose();
                    _db = null;
                    _isInitialized = false;
                    Debug.Log("[SQLite] Connection closed successfully.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] CloseConnection failed: {e.Message}");
            }
        }
    }
    ~DatabaseManager()
    {
        CloseConnection();
    }
}