using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System.Linq;

public class AdventureManager : MonoBehaviour
{
    //public EnemyDatabase enemyDatabase;
    public GameObject combatPanel;
    public GameObject adventurePanel;
    public CharacterStats playerStats;
    //public AdventureTextDatabase textDatabase;
    public TextMeshProUGUI adventureTMP;
    public EnemySOList enemySOList;
    public EnemyGroupData enemyGroup;
    public ItemGroupData itemGroupData;

    private List<AdventureTextData> textDatabase = new List<AdventureTextData>();

    // Adventure data
    public AdventureTextData currentAdventureTextData;
    public static AdventureManager Instance { get; private set; }

    // Map integration events
    public System.Action<string> OnAdventureChanged;
    public System.Action<string> OnLinkClicked;

    void Awake()
    {
        if (!Application.isPlaying) return;
        Instance = this;
        textDatabase = AdventureTextJsonImporter.Import();
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        string startAdventureId = DatabaseManager.Instance.GetAdventureProgress().CurrentAdventureId;
        // Initial load without triggering map movement
        AdventureTextData data = textDatabase.Find(t => t.id == startAdventureId);
        currentAdventureTextData = data;
        string targetMapId = data.mapId;
        Vector2Int targetPosition = data.mapPosition;
        if (data != null)
        {
            adventureTMP.text = data.text;

            if (PlayerMapVisualizer.Instance != null)
            {
                PlayerMapVisualizer.Instance.ShowPlayerAt(targetMapId, targetPosition);
            }
        }
    }

    public void OnEnemyLinkClicked(string linkID)
    {
        if (!Application.isPlaying) return;

        List<AddedEnemyData> enemyGroupList = enemyGroup.GetEnemiesFromGroup(linkID);
        if (enemyGroupList == null || enemyGroupList.Count == 0)
        {
            Debug.LogWarning($"No enemy found with id: {linkID}");
            return;
        }
        List<FighterData> fighterDataList = new List<FighterData>();
        foreach (AddedEnemyData enemyData in enemyGroupList)
        {
            EnemySO enemySO = enemySOList.GetEnemyById(enemyData.enemyID);
            FighterData model = new FighterData(enemySO);
            fighterDataList.Add(model);
        }

        TurnManager.Instance.SpawnDynamicEnemies(fighterDataList);
        TurnManager.Instance.currentAdventureText = currentAdventureTextData;

        adventurePanel.SetActive(false);
        combatPanel.SetActive(true);
    }

    public void ShowTextById(string id)
    {
        if (!Application.isPlaying) return;

        AdventureTextData data = textDatabase.Find(t => t.id == id);
        currentAdventureTextData = data; // Store the current text data for later use
        if (data == null)
        {
            Debug.LogWarning($"No adventure text found with id: {id}");
            return;
        }

        adventureTMP.text = data.text;
        //playerStats.SaveToDatabase();
        DatabaseManager.Instance.SaveAdventureProgress(currentAdventureTextData.id);

        // Map integration - notify map system (only once) - DISABLED TO PREVENT LOOP
        // OnAdventureChanged?.Invoke(id);
    }

    public void OnTextLinkClicked(string linkID)
    {
        if (!Application.isPlaying) return;

        //AdventureTextData data = textDatabase.Find(t => t.text.Contains($"<link=\"{linkID}\""));
        //currentAdventureTextData = data; // Store the current text data for later use
        //if (data == null)
        //{
        //    Debug.LogWarning($"No adventure text found that contains linkID: {linkID}");
        //    return;
        //}

        AdventureTextType resolvedType = AdventureTextType.Narration;

        if (currentAdventureTextData.links != null)
        {
            AdventureLink foundLink = currentAdventureTextData.links.Find(l => l.linkID == linkID);
            if (foundLink != null)
                resolvedType = foundLink.type;
        }

        // Map integration - notify map system before processing link
        OnLinkClicked?.Invoke(linkID);

        switch (resolvedType)
        {
            case AdventureTextType.Narration:
            case AdventureTextType.Dialog:
                // Move player first, then show text
                if (AdventureMapSyncer.Instance != null)
                {
                    var targetAdventure = GetAdventureById(linkID);
                    if (targetAdventure != null)
                    {
                        AdventureMapSyncer.Instance.MovePlayerToAdventure(targetAdventure);
                    }
                }
                ShowTextById(linkID);
                break;

            case AdventureTextType.Battle:
                OnEnemyLinkClicked(linkID);
                break;
            case AdventureTextType.Quest:
                QuestManager.Instance.AcceptQuest(linkID);
                break;                
            case AdventureTextType.Item:
                AddItemGroupToInventory(linkID);
                break;
        }
    }

    public void setCurrentAdventureTextData(string id)
    {
        if (!Application.isPlaying) return;

        currentAdventureTextData = textDatabase.Find(t => t.id == id);
    }

    // Map integration helper methods
    public AdventureTextData GetAdventureById(string adventureId)
    {
        return textDatabase.FirstOrDefault(a => a.id == adventureId);
    }

    public AdventureTextData GetAdventureAtPosition(string mapId, Vector2Int position)
    {
        return textDatabase.FirstOrDefault(a =>
            a.mapId == mapId && a.mapPosition == position);
    }

    public List<AdventureTextData> GetAdventuresForMap(string mapId)
    {
        return textDatabase.Where(a => a.mapId == mapId).ToList();
    }
    
    // Add items from item group to inventory
    private void AddItemGroupToInventory(string groupId)
    {
        if (itemGroupData == null)
        {
            Debug.LogError("ItemGroupData reference is null!");
            return;
        }
        
        Dictionary<string, int> itemsToAdd = itemGroupData.GetItemsFromGroup(groupId);
        
        if (itemsToAdd.Count == 0)
        {
            Debug.LogWarning($"No items found for group ID: {groupId}");
            return;
        }
        
        // Add each item to inventory
        foreach (var item in itemsToAdd)
        {
            string itemId = item.Key;
            int itemCount = item.Value;
            
            // Add to inventory using inventory system
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemId, itemCount);
            }
            else
            {
                Debug.LogError("InventoryManager instance is null!");
            }
        }
        
        Debug.Log($"Added items from group '{groupId}' to inventory");
    }
}