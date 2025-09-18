using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text amountText;
    public Image isEquipedBG;

    private InventoryItemModel currentItem;
    private Button button;
    public GameObject ItemDescriptionPanel;

    /// <summary>
    /// Initialize components for click detection
    /// </summary>
    private void Awake()
    {
        // Try to get Button component first
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnItemClicked);
        }
        else
        {
            // Enable raycast target on this GameObject for IPointerClickHandler
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
            }
        }
    }

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
    private void OnItemClicked()
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

    /// <summary>
    /// Handle pointer click events - implements IPointerClickHandler
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only handle clicks if no Button component is present
        if (button == null)
        {
            OnItemClicked();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnItemClicked);
        }
    }
}
