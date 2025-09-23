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
    public GameObject shopBuyPanel;
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
        LoadAdventure(startAdventureId, true);
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
        SkillManager.Instance.PopulateSkills();

        //adventurePanel.SetActive(false);
        combatPanel.SetActive(true);
    }

    public void ShowTextById(string id)
    {
        if (!Application.isPlaying) return;
        LoadAdventure(id, false);
    }

    private void LoadAdventure(string id, bool isInitialLoad)
    {
        AdventureTextData data = textDatabase.Find(t => t.id == id);
        currentAdventureTextData = data; // Store the current text data for later use
        if (data == null)
        {
            Debug.LogWarning($"No adventure text found with id: {id}");
            return;
        }

        // Check condition logic
        if (!string.IsNullOrEmpty(data.conditionId))
        {
            int currentConditionValue = GetConditionValue(data.conditionId);
            if (currentConditionValue >= data.conditionRequiredValue && !string.IsNullOrEmpty(data.conditionAdventureLink))
            {
                // Condition is true, redirect to different adventure
                LoadAdventure(data.conditionAdventureLink, isInitialLoad);
                return;
            }
        }

        // Add value to condition if specified
        if (!string.IsNullOrEmpty(data.addValueConditionId))
        {
            AddValueToCondition(data.addValueConditionId, data.addValue);
        }

        // Display main adventure text
        string displayText = data.text;
        
        // Add link texts below main text if they exist
        if (data.links != null && data.links.Count > 0)
        {
            displayText += "\n\n"; // Add some spacing
            foreach (AdventureLink link in data.links)
            {
                if (!string.IsNullOrEmpty(link.linkText))
                {
                    displayText += "<link=\"" + link.linkID + "\">" + link.linkText + "</link>\n";
                }
            }
        }
        
        adventureTMP.text = displayText;
        
        // Show player on map only during initial load
        if (isInitialLoad && PlayerMapVisualizer.Instance != null)
        {
            string targetMapId = data.mapId;
            Vector2Int targetPosition = data.mapPosition;
            PlayerMapVisualizer.Instance.ShowPlayerAt(targetMapId, targetPosition);
        }

        //playerStats.SaveToDatabase();
        DatabaseManager.Instance.SaveAdventureProgress(currentAdventureTextData.id);

        // Map integration - notify map system (only once) - DISABLED TO PREVENT LOOP
        // OnAdventureChanged?.Invoke(id);
    }

    public void OnTextLinkClicked(string linkID)
    {
        if (!Application.isPlaying) return;

        AdventureTextType resolvedType = AdventureTextType.Narration;
        string objectID = string.Empty;
        string nextLinkID = string.Empty;

        if (currentAdventureTextData.links != null)
        {
            AdventureLink foundLink = currentAdventureTextData.links.Find(l => l.linkID == linkID);
            if (foundLink != null)
            {
                // If the link is found, use its type
                resolvedType = foundLink.type;
                objectID = foundLink.objectID;
                nextLinkID = foundLink.nextLinkID;
            }
            else
            {
                Debug.LogWarning($"No link found with ID: {linkID} in current adventure text.");
            }
        }

        // Map integration - notify map system before processing link
        OnLinkClicked?.Invoke(nextLinkID);

        //Move player first, then show text
        if (AdventureMapSyncer.Instance != null)
        {
            var targetAdventure = GetAdventureById(nextLinkID);
            if (targetAdventure != null)
            {
                AdventureMapSyncer.Instance.MovePlayerToAdventure(targetAdventure);
            }
        }
        ShowTextById(nextLinkID);

        switch (resolvedType)
        {
            case AdventureTextType.Battle:
                OnEnemyLinkClicked(objectID);
                break;
            case AdventureTextType.Quest:
                QuestManager.Instance.AcceptQuest(objectID);
                break;                
            case AdventureTextType.AddItem:
                AddItemGroupToInventory(objectID);
                break;
            case AdventureTextType.AddComrade:
                AddComradeToParty(objectID);
                break;
            case AdventureTextType.RemoveComrade:
                RemoveComradeFromParty(objectID);
                break;
            case AdventureTextType.AddSkill:
                AddSkillToPlayer(objectID);
                break;
            case AdventureTextType.RemoveSkill:
                RemoveSkillFromPlayer(objectID);
                break;
            case AdventureTextType.BuyShop:
                OpenBuyShop(objectID);
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
            Debug.LogWarning($"Group ID: {groupId} has no item");
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
    
    // Condition system methods
    public int GetConditionValue(string conditionId)
    {
        return DatabaseManager.Instance.GetConditionValue(conditionId);
    }
    
    public void AddValueToCondition(string conditionId, int valueToAdd)
    {
        DatabaseManager.Instance.AddValueToCondition(conditionId, valueToAdd);
        Debug.Log($"Condition '{conditionId}' added value: {valueToAdd}");
    }
    
    public void SetConditionValue(string conditionId, int value)
    {
        DatabaseManager.Instance.SetConditionValue(conditionId, value);
        Debug.Log($"Condition '{conditionId}' set to value: {value}");
    }
    
    // Comrade management methods
    private void AddComradeToParty(string comradeId)
    {
        if (ComradeManager.Instance != null)
        {
            ComradeManager.Instance.AddComrade(comradeId);
        }
        else
        {
            Debug.LogError("ComradeManager instance is null!");
        }
    }
    
    private void RemoveComradeFromParty(string comradeId)
    {
        if (ComradeManager.Instance != null)
        {
            ComradeManager.Instance.RemoveComrade(comradeId);
        }
        else
        {
            Debug.LogError("ComradeManager instance is null!");
        }
    }
    
    // Skill management methods
    private void AddSkillToPlayer(string skillId)
    {
        if (playerStats != null)
        {
            bool success = playerStats.AddSkill(skillId);
            if (success)
            {
                Debug.Log($"Successfully added skill '{skillId}' to player");
            }
        }
        else
        {
            Debug.LogError("PlayerStats reference is null!");
        }
    }
    
    private void RemoveSkillFromPlayer(string skillId)
    {
        if (playerStats != null)
        {
            bool success = playerStats.RemoveSkill(skillId);
            if (success)
            {
                Debug.Log($"Successfully removed skill '{skillId}' from player");
            }
        }
        else
        {
            Debug.LogError("PlayerStats reference is null!");
        }
    }

    // Shop management methods
    private void OpenBuyShop(string shopID)
    {
        shopBuyPanelUI.SetCurrentShop(shopID);
        shopBuyPanel.SetActive(true);
    }

}