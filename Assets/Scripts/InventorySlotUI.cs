using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text amountText;
    public Image isEquipedBG;

    private InventoryItemModel currentItem;

    public void Setup(ItemSO item, InventoryItemModel inventoryItem)
    {
        if (!Application.isPlaying) return;
        
        currentItem = inventoryItem;
        iconImage.sprite = item.icon;
        amountText.text = inventoryItem.Amount.ToString();
        isEquipedBG.gameObject.SetActive(inventoryItem.IsEquipped);
    }

    public void OnItemClicked()
    {
        if (currentItem == null) return;
        
        // Trigger the panel activation event
        InventoryPanelUI.currentItem = currentItem;
        InventoryPanelUI.TriggerItemClick();
    }
}
