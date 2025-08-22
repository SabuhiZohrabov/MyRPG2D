using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Prefab and Container")]
    public GameObject fighterPrefab;           // The UI prefab for each fighter
    public Transform fighterContainer;         // The parent container (Content) where fighters will be added
    public GameObject skillPanel;

    [Header("List of Fighters")]
    public List<FighterData> fighterDataList = new List<FighterData>();         // Names for each fighter to be created

    [Header("Turn Settings")]
    public float highlightScale = 1.2f;        // Scale to highlight the active fighter

    public List<GameObject> fighterUIList = new List<GameObject>(); // Runtime list of fighters
    private int currentIndex = 0;
    private GameObject currentFighter;
    public AdventureTextData currentAdventureText;

    private FighterData cachedPlayer;
    private List<FighterData> defeatedEnemiesCache = new List<FighterData>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //SpawnFighters(); // Dynamically create fighter UI elements
        //if (fighterUIList.Count > 0)
        //    ActivateFighter(currentIndex); // Highlight the first fighter
    }

    public FighterData GetCurrentFighterModel()
    {
        if (currentIndex >= 0 && currentIndex < fighterDataList.Count)
        {
            return fighterDataList[currentIndex];
        }
        return null;
    }

    void SpawnFighters()
    {
        for (int i = 0; i < fighterDataList.Count; i++)
        {
            FighterData model = fighterDataList[i];

            GameObject fighter = Instantiate(fighterPrefab, fighterContainer);

            // Set fighter name based on type
            string fighterName = "Unknown";
            if (model != null)
                fighterName = "Player";

            fighter.name = fighterName;

            TextMeshProUGUI nameText = fighter.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = fighterName;

            FighterUI fighterUI = fighter.GetComponent<FighterUI>();
            if (fighterUI != null)
            {
                fighterUI.Setup(model);
            }

            // Add Button onClick listener dynamically
            Button btn = fighter.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (SkillManager.Instance != null)
                        SkillManager.Instance.OnTargetSelected(fighter);
                });
            }

            fighterUIList.Add(fighter);
        }
    }

    public void SpawnDynamicEnemies(List<FighterData> newEnemies)
    {
        var playerstat = GameObject.FindWithTag("Player").GetComponent<CharacterStats>();
        if (playerstat == null)
        {
            Debug.LogError("Player FighterModel not found!");
            return;
        }
        var player = new FighterData(playerstat);
        // fill fighterDataList with player, comrades and new enemies
        fighterDataList.Clear();
        fighterDataList.Add(player);
        
        // Add active comrades to battle
        if (ComradeManager.Instance != null)
        {
            List<FighterData> activeComrades = ComradeManager.Instance.GetActiveComradesForBattle();
            fighterDataList.AddRange(activeComrades);
            Debug.Log($"Added {activeComrades.Count} comrades to battle");
        }
        
        fighterDataList.AddRange(newEnemies);

        // Recreate UI (fighters and models)
        ClearFighterUI();
        fighterUIList.Clear();
        SpawnFighters();

        // clear combat log
        CombatLog.Instance.ClearAll();

        currentIndex = -1;
        NextTurn();
    }

    public void NextTurn()
    {
        if (!IsPlayerTeamAlive())
        {
            CombatLog.Instance.AddLog("<color=red>You have been defeated.</color>");
            skillPanel.SetActive(false);
            UIManager.Instance.ShowEndGame("You Lose!");
            return;
        }

        if (!IsEnemyTeamAlive())
        {
            CombatLog.Instance.AddLog("<color=green>You are victorious!</color>");
            skillPanel.SetActive(false);
            OnVictory();
            return;
        }

        if (fighterUIList.Count == 0) return;

        int startingIndex = currentIndex;
        FighterData model;
        do
        {
            // Reset previous fighter scale
            if (currentFighter != null)
                currentFighter.transform.localScale = Vector3.one;

            // Move to next
            currentIndex = (currentIndex + 1) % fighterUIList.Count;
            model = fighterDataList[currentIndex];

            // If alive --> break the loop
            if (model.isAlive)
                break;

            // If we loop back to starting index and no alive found --> stop
            if (currentIndex == startingIndex)
            {
                Debug.Log("No alive fighters left.");
                return;
            }

        } while (true);

        if (model.isPlayer)
        {
            SkillManager.Instance.ReduceCooldowns();
        }
        else if (model.isComrade)
        {
            // Comrades can use basic attack automatically
            // For now, comrades will skip their turn or use basic attack
            // You can implement AI logic here later
        }
        ActivateFighter(currentIndex);
    }

    void ActivateFighter(int index)
    {
        currentFighter = fighterUIList[index];
        FighterData model = fighterDataList[index];
        currentFighter.transform.localScale = Vector3.one * highlightScale;
        // Only show skill panel for the player
        skillPanel.SetActive(model.isPlayer);
        Debug.Log($"Now acting: {currentFighter.name}");
    }

    public int GetFighterIndex(GameObject fighterGO)
    {
        return fighterUIList.IndexOf(fighterGO);
    }

    public FighterData GetFighterModel(int index)
    {
        if (index >= 0 && index < fighterDataList.Count)
            return fighterDataList[index];
        return null;
    }

    public bool IsPlayerTeamAlive()
    {
        for (int i = 0; i < fighterDataList.Count; i++)
        {
            var model = fighterDataList[i];
            if (model.isPlayer && model.isAlive)
                return true;
        }
        return false;
    }

    public bool IsEnemyTeamAlive()
    {
        for (int i = 0; i < fighterDataList.Count; i++)
        {
            var model = fighterDataList[i];
            if (model.isEnemy && model.isAlive)
                return true;
        }
        return false;
    }

    private void ClearFighterUI()
    {
        foreach (Transform child in fighterContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private FighterData GetPlayerFighter()
    {
        if (cachedPlayer != null && cachedPlayer.isPlayer)
            return cachedPlayer;

        for (int i = 0; i < fighterDataList.Count; i++)
        {
            var fighter = fighterDataList[i];
            if (fighter.isPlayer)
            {
                cachedPlayer = fighter;
                return fighter;
            }
        }
        return null;
    }

    private void GetDefeatedEnemies(List<FighterData> resultList)
    {
        resultList.Clear();

        for (int i = 0; i < fighterDataList.Count; i++)
        {
            var fighter = fighterDataList[i];
            if (fighter.isEnemy && !fighter.isAlive)
            {
                resultList.Add(fighter);
            }
        }
    }

    private void OnVictory()
    {
        Debug.Log("Victory!");

        FighterData player = GetPlayerFighter();
        GetDefeatedEnemies(defeatedEnemiesCache);

        int earnedXP = 0;
        var lootBuilder = new System.Text.StringBuilder();

        // Loop through defeated enemies and process loot
        for (int i = 0; i < defeatedEnemiesCache.Count; i++)
        {
            var enemy = defeatedEnemiesCache[i];
            var enemySO = enemy.fighter as EnemySO;
            if (enemySO == null || enemySO.lootTable == null) continue;
            earnedXP += enemySO.expReward;

            var lootTable = enemySO.lootTable;
            for (int j = 0; j < lootTable.Count; j++)
            {
                var entry = lootTable[j];
                float roll = Random.value; // 0..1
                if (roll <= entry.dropChance)
                {
                    int amount = Random.Range(entry.minAmount, entry.maxAmount);

                    // Add to inventory
                    InventoryManager.Instance.AddItem(entry.item.itemId, amount);

                    // Collect for log
                    string log = $"<color=yellow>Loot: {entry.item.displayName}</color> x{amount}";
                    CombatLog.Instance.AddLog(log);
                    lootBuilder.AppendLine(log);
                }
            }
        }

        // Get final loot summary
        string lootSummary = lootBuilder.ToString();

        // Loot log
        if (string.IsNullOrEmpty(lootSummary))
        {
            lootSummary = "<color=grey>No loot dropped.</color>";
            CombatLog.Instance.AddLog(lootSummary);
        }

        // XP
        if (player != null && player.fighter is CharacterStats characterStats)
        {
            characterStats.GainXP(earnedXP);
            Debug.Log($"Player gained {earnedXP} XP!");

            UIManager.Instance.ShowWonGame("You Win!", earnedXP, lootSummary);
        }
    }
}