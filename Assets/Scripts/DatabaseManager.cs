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

    // Generic transaction wrapper methods for better code organization
    private T ExecuteWithTransaction<T>(Func<T> operation, T defaultValue)
    {
        if (_db == null) return defaultValue;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    T result = operation();
                    _db.Commit();
                    return result;
                }
                catch
                {
                    _db.Rollback();
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SQLite] Transaction failed: {e.Message}");
                return defaultValue;
            }
        }
    }

    private void ExecuteWithTransaction(Action operation, string operationName)
    {
        if (_db == null) return;

        lock (_lockObject)
        {
            try
            {
                _db.BeginTransaction();
                try
                {
                    operation();
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
                Debug.LogError($"[SQLite] {operationName} failed: {e.Message}");
            }
        }
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
        ExecuteWithTransaction(() =>
        {
            if (stats.Id == 0)
                _db.Insert(stats);
            else
                _db.Update(stats);
        }, "SavePlayerStats");
    }

    public void DeleteAllData()
    {
        ExecuteWithTransaction(() =>
        {
            _db.DeleteAll<PlayerStatsModel>();
        }, "DeleteAllData");
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
        // Use the more efficient bulk version
        SavePlayerSkillsBulk(playerId, skills);
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

    // Bulk operations for better performance
    public void SavePlayerSkillsBulk(int playerId, List<string> skills)
    {
        ExecuteWithTransaction(() =>
        {
            // Use single DELETE query instead of loop
            _db.Execute("DELETE FROM PlayerSkillModel WHERE PlayerId = ?", playerId);
            
            // Use bulk insert
            if (skills != null && skills.Count > 0)
            {
                _db.InsertAll(skills.Select(skillName => new PlayerSkillModel(playerId, skillName)));
            }
        }, "SavePlayerSkillsBulk");
    }

    public void DeleteAllPlayerSkills(int playerId)
    {
        ExecuteWithTransaction(() =>
        {
            _db.Execute("DELETE FROM PlayerSkillModel WHERE PlayerId = ?", playerId);
        }, "DeleteAllPlayerSkills");
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
        ExecuteWithTransaction(() =>
        {
            _db.Insert(item);
        }, "InsertInventoryItem");
    }

    public void UpdateInventoryItem(InventoryItemModel item)
    {
        ExecuteWithTransaction(() =>
        {
            _db.Update(item);
        }, "UpdateInventoryItem");
    }

    public void DeleteInventoryItem(string itemId)
    {
        ExecuteWithTransaction(() =>
        {
            var existing = GetInventoryItem(itemId);
            if (existing != null)
            {
                _db.Delete(existing);
            }
        }, "DeleteInventoryItem");
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

    // Bulk inventory operations for better performance
    public void InsertInventoryItemsBulk(List<InventoryItemModel> items)
    {
        if (items == null || items.Count == 0) return;
        
        ExecuteWithTransaction(() =>
        {
            _db.InsertAll(items);
        }, "InsertInventoryItemsBulk");
    }

    public void UpdateInventoryItemsBulk(List<InventoryItemModel> items)
    {
        if (items == null || items.Count == 0) return;
        
        ExecuteWithTransaction(() =>
        {
            _db.UpdateAll(items);
        }, "UpdateInventoryItemsBulk");
    }

    public void DeleteInventoryItemsBulk(List<string> itemIds)
    {
        if (itemIds == null || itemIds.Count == 0) return;
        
        ExecuteWithTransaction(() =>
        {
            string placeholders = string.Join(",", itemIds.Select(_ => "?"));
            _db.Execute($"DELETE FROM InventoryItemModel WHERE ItemId IN ({placeholders})", itemIds.ToArray());
        }, "DeleteInventoryItemsBulk");
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
        ExecuteWithTransaction(() =>
        {
            var progress = GetAdventureProgress();
            progress.CurrentAdventureId = currentAdventureId;
            _db.InsertOrReplace(progress);
        }, "SaveAdventureProgress");
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
        ExecuteWithTransaction(() =>
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
        }, "SetConditionValue");
    }

    public void AddValueToCondition(string conditionId, int valueToAdd)
    {
        ExecuteWithTransaction(() =>
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
        }, "AddValueToCondition");
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

    // Bulk condition operations for better performance
    public void SetConditionsBulk(Dictionary<string, int> conditions)
    {
        if (conditions == null || conditions.Count == 0) return;

        ExecuteWithTransaction(() =>
        {
            foreach (var condition in conditions)
            {
                _db.InsertOrReplace(new ConditionModel(condition.Key, condition.Value));
            }
        }, "SetConditionsBulk");
    }

    public void DeleteConditionsBulk(List<string> conditionIds)
    {
        if (conditionIds == null || conditionIds.Count == 0) return;

        ExecuteWithTransaction(() =>
        {
            string placeholders = string.Join(",", conditionIds.Select(_ => "?"));
            _db.Execute($"DELETE FROM ConditionModel WHERE ConditionId IN ({placeholders})", conditionIds.ToArray());
        }, "DeleteConditionsBulk");
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