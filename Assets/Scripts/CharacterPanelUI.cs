using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelUI : MonoBehaviour
{
    public GameObject attributesPanel;
    public GameObject inventoryPanel;
    public GameObject skillsPanel;

    //private void Start()
    //{
    //    if (!Application.isPlaying) return;
    //    ToggleAllPanels(false);
    //}

    public void ToggleAttributesPanel()
    {
        attributesPanel.SetActive(!attributesPanel.activeSelf);

        if (attributesPanel.activeSelf)
        {
            var panelScript = attributesPanel.GetComponent<AttributesPanelUI>();
            panelScript?.RefreshUI();
        }
    }
    public void ToggleInventoryPanel()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        if (inventoryPanel.activeSelf)
        {
            var panelScript = inventoryPanel.GetComponent<InventoryPanelUI>();
            panelScript?.RefreshUI();
        }
    }
    private void ToggleAllPanels(bool isOpen)
    {
        attributesPanel.SetActive(isOpen);
        inventoryPanel.SetActive(isOpen);
        skillsPanel.SetActive(isOpen);
    }
    public void ToggleSkillsPanel()
    {
        skillsPanel.SetActive(!skillsPanel.activeSelf);

        if (skillsPanel.activeSelf)
        {
            var panelScript = skillsPanel.GetComponent<SkillsPanelUI>();
            panelScript?.RefreshUI();
        }
    }
}
