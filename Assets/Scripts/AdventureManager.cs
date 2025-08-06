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

            // Only move player on initial load if adventure has map data
            //if (!string.IsNullOrEmpty(data.mapId) && AdventureMapSyncer.Instance != null)
            //{
            //    AdventureMapSyncer.Instance.MovePlayerToAdventure(data);
            //}
            if (PlayerMapVisualizer.Instance != null)
            {
                PlayerMapVisualizer.Instance.ShowPlayerAt(targetMapId, targetPosition);
            }
        }
    }

    public void OnEnemyLinkClicked(string linkID)
    {
        if (!Application.isPlaying) return;

        EnemySO enemySO = enemySOList.GetEnemyById(linkID);
        if (enemySO == null)
        {
            Debug.LogWarning($"No enemy found with id: {linkID}");
            return;
        }

        FighterData model = new FighterData(enemySO);
        TurnManager.Instance.SpawnDynamicEnemies(new List<FighterData> { model });
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
}