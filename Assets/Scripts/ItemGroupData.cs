using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class AddedItemData
{
    public string itemId;
    public int minCount = 0; // Minimum count for this item (0 means can be skipped)
    public int maxCount = 1; // Maximum count for this item
}

[System.Serializable]
public class ItemGroup
{
    public string groupId;
    public string groupName;
    public List<AddedItemData> items = new List<AddedItemData>();
}

[System.Serializable]
public class ItemGroupContainer
{
    public List<ItemGroup> itemGroups = new List<ItemGroup>();
}

[CreateAssetMenu(fileName = "ItemGroupData", menuName = "RPG/Item Group Data")]
public class ItemGroupData : ScriptableObject
{
    public List<ItemGroup> itemGroups = new List<ItemGroup>();
    
    private string JsonFilePath => Path.Combine(Application.streamingAssetsPath, StoryManager.SelectedStoryId + "ItemGroups.json");
    
    // Get item group by ID
    public ItemGroup GetItemGroup(string groupId)
    {
        foreach (ItemGroup group in itemGroups)
        {
            if (group.groupId == groupId)
                return group;
        }
        return null;
    }
    
    // Get items from group with random count for each item
    public Dictionary<string, int> GetItemsFromGroup(string groupId)
    {
        ItemGroup group = GetItemGroup(groupId);
        if (group == null || group.items.Count == 0)
            return new Dictionary<string, int>();
            
        Dictionary<string, int> selectedItems = new Dictionary<string, int>();
        
        foreach (AddedItemData itemData in group.items)
        {
            // Generate random count for this item
            int count = itemData.minCount;

            if (itemData.maxCount > itemData.minCount)
            {
                count = Random.Range(itemData.minCount, itemData.maxCount+1);
            }

            // If count is greater than 0, add to result
            if (count > 0)
            {
                selectedItems[itemData.itemId] = count;
            }
        }
        
        return selectedItems;
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
            
            ItemGroupContainer container = new ItemGroupContainer();
            container.itemGroups = itemGroups;
            
            string jsonData = JsonUtility.ToJson(container, true);
            File.WriteAllText(JsonFilePath, jsonData);
            
            Debug.Log($"Item groups saved to: {JsonFilePath}");
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save item groups: {e.Message}");
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
                ItemGroupContainer container = JsonUtility.FromJson<ItemGroupContainer>(jsonData);
                
                if (container != null)
                {
                    itemGroups = container.itemGroups;
                    Debug.Log($"Item groups loaded from: {JsonFilePath}");
                    
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
            Debug.LogError($"Failed to load item groups: {e.Message}");
        }
    }
}