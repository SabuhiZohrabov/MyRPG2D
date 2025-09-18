using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public GameObject ItemDescriptionPanel;


    public Image iconImage;
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;

    public static InventoryItemModel currentItem;


    // Static event for item click communication
    public static event Action OnItemClickedEvent;

    private void OnEnable()
    {        
        // Subscribe to the item click event
        OnItemClickedEvent += CloseDescriptionPanel;
        OnItemClickedEvent += SetItemInfos;

        // Refresh UI when panel becomes active
        RefreshUI();
        ItemDescriptionPanel.SetActive(false);
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks
        OnItemClickedEvent -= CloseDescriptionPanel;
        OnItemClickedEvent -= SetItemInfos;
    }

    public void RefreshUI()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }


        List<InventoryItemModel> items = InventoryManager.Instance.GetAllItems();

        if (ItemDatabase.Instance == null) return;

        foreach (var model in items)
        {
            if (model == null || string.IsNullOrEmpty(model.ItemId)) continue;

            ItemSO itemSO = ItemDatabase.Instance.GetItemById(model.ItemId);

            if (itemSO == null) continue;

            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
            slot.GetComponent<InventorySlotUI>().Setup(itemSO, model);
        }
    }

    public void CloseDescriptionPanel()
    {
        if (ItemDescriptionPanel != null)
        {
            ItemDescriptionPanel.SetActive(!ItemDescriptionPanel.activeSelf);
        }
    }
    
    public void SetItemInfos()
    {
        if (currentItem == null || string.IsNullOrEmpty(currentItem.ItemId)) return;
        ItemSO itemSO = ItemDatabase.Instance.GetItemById(currentItem.ItemId);
        if (itemSO == null) return;
        iconImage.sprite = itemSO.icon;
        itemNameText.text = itemSO.displayName;
        itemDescText.text = itemSO.description;
    }

    public void EquipItem()
    {
        if (currentItem == null) return;
        bool wasEquipped = InventoryManager.Instance.IsItemEquipped(currentItem.Id);
        bool success = false;
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
            RefreshUI();
        }
    }

    /// <summary>
    /// Static method to trigger the item click event
    /// </summary>
    public static void TriggerItemClick()
    {
        OnItemClickedEvent?.Invoke();
    }
}
