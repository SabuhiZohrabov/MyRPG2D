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
                _db.CreateTable<InventoryItemModel>();
                _db.CreateTable<AdventureProgressModel>();

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
        stats.SetDatabaseId(saved.Id);
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