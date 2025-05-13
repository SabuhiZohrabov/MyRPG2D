using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [Header("Prefab and Container")]
    public GameObject fighterPrefab;           // The UI prefab for each fighter
    public Transform fighterContainer;         // The parent container (Content) where fighters will be added
    public GameObject skillPanel;

    [Header("List of Fighters")]
    public List<FighterModel> fighterDataList = new List<FighterModel>();         // Names for each fighter to be created

    [Header("Turn Settings")]
    public float highlightScale = 1.2f;        // Scale to highlight the active fighter

    private List<GameObject> fighterUIList = new List<GameObject>(); // Runtime list of fighters
    private int currentIndex = 0;
    private GameObject currentFighter;

    void Start()
    {
        SpawnFighters(); // Dynamically create fighter UI elements
        if (fighterUIList.Count > 0)
            ActivateFighter(currentIndex); // Highlight the first fighter
    }

    void SpawnFighters()
    {
        for (int i = 0; i < fighterDataList.Count; i++)
        {
            FighterModel model = fighterDataList[i];

            GameObject fighter = Instantiate(fighterPrefab, fighterContainer);
            fighter.name = model.name;

            TextMeshProUGUI nameText = fighter.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = model.name;

            fighterUIList.Add(fighter);
        }
    }

    public void NextTurn()
    {
        if (fighterUIList.Count == 0) return;

        // Reset previous fighter scale
        if (currentFighter != null)
            currentFighter.transform.localScale = Vector3.one;

        // Move to next fighter
        currentIndex = (currentIndex + 1) % fighterUIList.Count;
        ActivateFighter(currentIndex);
    }

    void ActivateFighter(int index)
    {
        currentFighter = fighterUIList[index];
        FighterModel model = fighterDataList[index];
        currentFighter.transform.localScale = Vector3.one * highlightScale;
        // Only show skill panel for the player
        if (model.isPlayer)
        {
            skillPanel.SetActive(true);
        }
        else
        {
            skillPanel.SetActive(false);
        }
        Debug.Log($"Now acting: {currentFighter.name}");
    }
}
