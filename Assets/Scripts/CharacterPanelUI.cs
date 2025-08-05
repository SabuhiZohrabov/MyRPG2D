using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelUI : MonoBehaviour
{
    public Image avatarImage;
    public TMP_Text nameText;
    public CharacterStats characterStats;

    public GameObject attributesPanel;
    public GameObject inventoryPanel;
    public GameObject skillsPanel;

    private void Start()
    {
        if (!Application.isPlaying) return;
        ToggleAllPanels(false);

        // Show avatar and name
        if (avatarImage != null && characterStats != null)
        {
            if (characterStats.AvatarSprite != null)
            {
                avatarImage.sprite = characterStats.AvatarSprite;
                avatarImage.color = Color.white;
                avatarImage.preserveAspect = true; // Maintain aspect ratio
            }
            else
            {
                avatarImage.color = new Color(1, 1, 1, 0.2f); // Semi-transparent if no avatar
            }
        }
        if (nameText != null && characterStats != null)
        {
            nameText.text = characterStats.DisplayName;
        }
    }

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
