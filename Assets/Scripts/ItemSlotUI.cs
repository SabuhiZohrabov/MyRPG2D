using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text amountText;
    public Image isEquipedBG;

    private ItemSO currentItem;
    private Button button;

    /// <summary>
    /// for click detection
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
            // If no Button, add click detection to iconImage (ItemIcon)
            if (iconImage != null)
            {
                iconImage.raycastTarget = true;
                
                // Add EventTrigger for click detection
                var eventTrigger = iconImage.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = iconImage.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }

                // Add click event
                var clickEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                clickEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                clickEntry.callback.AddListener((data) => OnItemClicked());
                eventTrigger.triggers.Add(clickEntry);
            }
        }
    }

    public void Setup(ItemSO item, int amount, bool isEquiped)
    {
        if (!Application.isPlaying) return;
        
        currentItem = item;
        iconImage.sprite = item.icon;
        nameText.text = item.displayName;
        amountText.text = amount.ToString();
        isEquipedBG.gameObject.SetActive(isEquiped);
    }


    /// <summary>
    /// for testing purpose, simply equip/unequip item on click
    /// </summary>
    private void OnItemClicked()
    {
        if (currentItem == null) return;

        bool success = false;
        bool wasEquipped = InventoryManager.Instance.IsItemEquipped(currentItem.itemId);

        if (wasEquipped)
        {
            success = InventoryManager.Instance.UnequipItem(currentItem.itemId);
        }
        else
        {
            success = InventoryManager.Instance.EquipItem(currentItem.itemId);
        }

        if (success)
        {
            FindFirstObjectByType<InventoryPanelUI>().RefreshUI();
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
