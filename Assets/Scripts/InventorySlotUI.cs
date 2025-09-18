using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text amountText;
    public Image isEquipedBG;

    private InventoryItemModel currentItem;
    private Button button;
    public GameObject ItemDescriptionPanel;

    public void Setup(ItemSO item, InventoryItemModel inventoryItem)
    {
        if (!Application.isPlaying) return;
        
        currentItem = inventoryItem;
        iconImage.sprite = item.icon;
        amountText.text = inventoryItem.Amount.ToString();
        isEquipedBG.gameObject.SetActive(inventoryItem.IsEquipped);
    }


    /// <summary>
    /// for testing purpose, simply equip/unequip item on click
    /// </summary>
    public void OnItemClicked()
    {
        if (currentItem == null) return;

        bool success = false;
        bool wasEquipped = InventoryManager.Instance.IsItemEquipped(currentItem.Id);

        if (wasEquipped)
        {
            success = InventoryManager.Instance.UnequipItem(currentItem.Id);
        }
        else
        {
            success = InventoryManager.Instance.EquipItem(currentItem.Id);
        }

        if (success)
        {
            FindFirstObjectByType<InventoryPanelUI>().RefreshUI();
        }

        ItemDescriptionPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnItemClicked);
        }
    }
}
