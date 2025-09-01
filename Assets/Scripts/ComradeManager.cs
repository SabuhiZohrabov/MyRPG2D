using System.Collections.Generic;
using UnityEngine;

public class ComradeManager : MonoBehaviour
{
    public static ComradeManager Instance { get; private set; }
    
    [Header("Comrade Database")]
    public List<ComradeData> allComrades = new List<ComradeData>();
    
    [Header("Active Comrades")]
    private List<string> activeComradeIds = new List<string>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load active comrades from database on start
        LoadActiveComradesFromDatabase();
    }
    
    // Add comrade to active list
    public void AddComrade(string comradeId)
    {
        if (string.IsNullOrEmpty(comradeId))
        {
            Debug.LogWarning("ComradeManager: Attempted to add comrade with empty ID");
            return;
        }
        
        ComradeData comradeData = GetComradeById(comradeId);
        if (comradeData == null)
        {
            Debug.LogWarning($"ComradeManager: Comrade with ID '{comradeId}' not found in database");
            return;
        }
        
        if (!activeComradeIds.Contains(comradeId))
        {
            activeComradeIds.Add(comradeId);
            DatabaseManager.Instance.AddActiveComrade(comradeId);
            Debug.Log($"ComradeManager: Added comrade '{comradeData.displayName}' to active list");
        }
        else
        {
            Debug.Log($"ComradeManager: Comrade '{comradeData.displayName}' is already active");
        }
    }
    
    // Remove comrade from active list
    public void RemoveComrade(string comradeId)
    {
        if (string.IsNullOrEmpty(comradeId))
        {
            Debug.LogWarning("ComradeManager: Attempted to remove comrade with empty ID");
            return;
        }
        
        ComradeData comradeData = GetComradeById(comradeId);
        if (comradeData != null && activeComradeIds.Contains(comradeId))
        {
            activeComradeIds.Remove(comradeId);
            DatabaseManager.Instance.RemoveActiveComrade(comradeId);
            Debug.Log($"ComradeManager: Removed comrade '{comradeData.displayName}' from active list");
        }
        else
        {
            Debug.Log($"ComradeManager: Comrade with ID '{comradeId}' is not in active list");
        }
    }
    
    // Get all active comrades as FighterData for battle
    public List<FighterData> GetActiveComradesForBattle()
    {
        List<FighterData> comradeFighters = new List<FighterData>();
        
        foreach (string comradeId in activeComradeIds)
        {
            ComradeData comradeData = GetComradeById(comradeId);
            if (comradeData != null)
            {
                FighterData fighterData = new FighterData(comradeData);
                comradeFighters.Add(fighterData);
            }
        }
        
        return comradeFighters;
    }
    
    // Get comrade data by ID
    private ComradeData GetComradeById(string comradeId)
    {
        return allComrades.Find(c => c.comradeId == comradeId);
    }
    
    // Check if comrade is active
    public bool IsComradeActive(string comradeId)
    {
        return activeComradeIds.Contains(comradeId);
    }
    
    // Get all active comrade IDs
    public List<string> GetActiveComradeIds()
    {
        return new List<string>(activeComradeIds);
    }
    
    // Get all active comrade data
    public List<ComradeData> GetActiveComrades()
    {
        List<ComradeData> activeComrades = new List<ComradeData>();
        
        foreach (string comradeId in activeComradeIds)
        {
            ComradeData comradeData = GetComradeById(comradeId);
            if (comradeData != null)
            {
                activeComrades.Add(comradeData);
            }
        }
        
        return activeComrades;
    }
    
    // Clear all active comrades (for debugging or reset)
    public void ClearAllComrades()
    {
        activeComradeIds.Clear();
        DatabaseManager.Instance.ClearActiveComrades();
        Debug.Log("ComradeManager: Cleared all active comrades");
    }

    // -----------------------
    // Database Integration
    // -----------------------

    // Load active comrades from database
    public void LoadActiveComradesFromDatabase()
    {
        try
        {
            List<string> savedComradeIds = DatabaseManager.Instance.GetActiveComradeIds();
            activeComradeIds = new List<string>(savedComradeIds);
            Debug.Log($"ComradeManager: Loaded {activeComradeIds.Count} active comrades from database");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ComradeManager: Failed to load active comrades from database: {e.Message}");
            activeComradeIds = new List<string>();
        }
    }

    // Save active comrades to database
    public void SaveActiveComradesToDatabase()
    {
        try
        {
            DatabaseManager.Instance.SaveActiveComrades(activeComradeIds);
            Debug.Log($"ComradeManager: Saved {activeComradeIds.Count} active comrades to database");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ComradeManager: Failed to save active comrades to database: {e.Message}");
        }
    }
}