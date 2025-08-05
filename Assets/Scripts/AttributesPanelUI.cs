using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class AttributesPanelUI : MonoBehaviour
{
    public CharacterStats stats;
    public TMP_Text pointsText;
    public Transform attributesContainer;
    public GameObject attributeRowPrefab;
    public TMP_Text levelText;
    public TMP_Text xpText;

    private Dictionary<string, AttributeRowUI> rowUIs = new();

    private void Start()
    {
        if (!Application.isPlaying) return;
        RenderAttributes();
    }

    public void RefreshUI()
    {
        RenderAttributes();

        pointsText.text = $"Available Points: {stats.AvailableAttributePoints}";
        levelText.text = $"Level: {stats.Level}";
        xpText.text = $"XP: {stats.CurrentXP} / {stats.XPToNextLevel}";

        foreach (var pair in stats.GetAllAttributes())
        {
            string attrName = pair.Key;
            int value = pair.Value.Value;
            bool canAdd = stats.CanIncrease(attrName);
            rowUIs[attrName].UpdateValue(value, canAdd);
        }
    }

    private void RenderAttributes()
    {
        if (rowUIs.Count == 0)
            foreach (var pair in stats.GetAllAttributes())
            {
                string attrName = pair.Key;
                int value = pair.Value.Value;
                bool canAdd = stats.CanIncrease(attrName);

                var obj = Instantiate(attributeRowPrefab, attributesContainer);
                var row = obj.GetComponent<AttributeRowUI>();
                row.Setup(attrName, value, canAdd, OnAddAttribute);
                rowUIs[attrName] = row;
            }
    }

    private void OnAddAttribute(string attributeName)
    {
        if (stats.IncreaseAttribute(attributeName))
        {
            stats.SaveToDatabase();
            RefreshUI();
        }
    }
}
