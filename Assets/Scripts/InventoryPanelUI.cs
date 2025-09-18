using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryPanelUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public GameObject ItemDescriptionPanel;
    
    
    // Static event for item click communication
    public static event Action OnItemClickedEvent;

    private void OnEnable()
    {        
        // Subscribe to the item click event
        OnItemClickedEvent += CloseDescriptionPanel;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks
        OnItemClickedEvent -= CloseDescriptionPanel;
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
        
    /// <summary>
    /// Static method to trigger the item click event
    /// </summary>
    public static void TriggerItemClick()
    {
        OnItemClickedEvent?.Invoke();
    }
}
