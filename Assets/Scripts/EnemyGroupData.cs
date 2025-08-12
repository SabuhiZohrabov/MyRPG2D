using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class AddedEnemyData
{
    public string enemyID;
    public int spawnWeight = 1; 
}

[System.Serializable]
public class EnemyGroup
{
    public string groupId;
    public string groupName;
    public List<AddedEnemyData> enemies = new List<AddedEnemyData>();
    public bool randomSelection = false; // Enable random selection from group
    public int minEnemyCount = 1; // Minimum number of enemies to spawn from group
    public int maxEnemyCount = 3; // Maximum number of enemies to spawn from group
}

[System.Serializable]
public class EnemyGroupContainer
{
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
}

[CreateAssetMenu(fileName = "EnemyGroupData", menuName = "RPG/Enemy Group Data")]
public class EnemyGroupData : ScriptableObject
{
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    
    private string JsonFilePath => Path.Combine(Application.streamingAssetsPath, StoryManager.SelectedStoryId + "EnemyGroups.json");
    
    // Get enemy group by ID
    public EnemyGroup GetEnemyGroup(string groupId)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.groupId == groupId)
                return group;
        }
        return null;
    }
    
    // Get enemies from group based on group settings
    public List<AddedEnemyData> GetEnemiesFromGroup(string groupId)
    {
        EnemyGroup group = GetEnemyGroup(groupId);
        if (group == null || group.enemies.Count == 0)
            return new List<AddedEnemyData>();
            
        List<AddedEnemyData> selectedEnemies = new List<AddedEnemyData>();
        
        // Determine how many enemies to spawn
        int enemiesToSpawn = Random.Range(group.minEnemyCount, group.maxEnemyCount + 1);
        
        if (group.randomSelection)
        {
            // Random selection with weight
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                AddedEnemyData selectedEnemy = SelectEnemyByWeight(group.enemies);
                selectedEnemies.Add(selectedEnemy);
            }
        }
        else
        {
            selectedEnemies = group.enemies;
        }
        
        return selectedEnemies;
    }
    
    // Select enemy based on spawn weight
    private AddedEnemyData SelectEnemyByWeight(List<AddedEnemyData> enemies)
    {
        int totalWeight = 0;
        foreach (AddedEnemyData enemy in enemies)
        {
            totalWeight += enemy.spawnWeight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (AddedEnemyData enemy in enemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue < currentWeight)
                return enemy;
        }
        
        return enemies[0]; // Fallback
    }
    
    // Save groups to JSON file
    [ContextMenu("Save to JSON")]
    public void SaveToJson()
    {
        try
        {
            // Ensure StreamingAssets folder exists
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            
            EnemyGroupContainer container = new EnemyGroupContainer();
            container.enemyGroups = enemyGroups;
            
            string jsonData = JsonUtility.ToJson(container, true);
            File.WriteAllText(JsonFilePath, jsonData);
            
            Debug.Log($"Enemy groups saved to: {JsonFilePath}");
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save enemy groups: {e.Message}");
        }
    }
    
    // Load groups from JSON file
    [ContextMenu("Load from JSON")]
    public void LoadFromJson()
    {
        try
        {
            if (File.Exists(JsonFilePath))
            {
                string jsonData = File.ReadAllText(JsonFilePath);
                EnemyGroupContainer container = JsonUtility.FromJson<EnemyGroupContainer>(jsonData);
                
                if (container != null)
                {
                    enemyGroups = container.enemyGroups;
                    Debug.Log($"Enemy groups loaded from: {JsonFilePath}");
                    
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
                    #endif
                }
            }
            else
            {
                Debug.LogWarning($"JSON file not found: {JsonFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load enemy groups: {e.Message}");
        }
    }
    
    // Auto-save when data changes in editor
    //private void OnValidate()
    //{
    //    #if UNITY_EDITOR
    //    if (!Application.isPlaying)
    //    {
    //        UnityEditor.EditorApplication.delayCall += () =>
    //        {
    //            if (this != null)
    //                SaveToJson();
    //        };
    //    }
    //    #endif
    //}
}