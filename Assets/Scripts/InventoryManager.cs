using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (!Application.isPlaying) return null;
            return _instance ?? (_instance = new InventoryManager());
        }
    }

    private InventoryManager() { }

    public void AddItem(string itemId, int amount = 1)
    {
        ItemSO itemSO = ItemDatabase.Instance.GetItemById(itemId);
        if (itemSO == null)
        {
            Debug.LogWarning($"Cannot add item. itemId not found: {itemId}");
            return;
        }

        if (itemSO.maxStack <= 1)
        {
            for (int i = 0; i < amount; i++)
            {
                var newItem = new InventoryItemModel
                {
                    ItemId = itemId,
                    Amount = 1
                };
                DatabaseManager.Instance.InsertInventoryItem(newItem);
            }

            return;
        }

        var existing = DatabaseManager.Instance.GetInventoryItem(itemId);
        if (existing != null)
        {
            existing.Amount += amount;
            DatabaseManager.Instance.UpdateInventoryItem(existing);
        }
        else
        {
            var newItem = new InventoryItemModel
            {
                ItemId = itemId,
                Amount = amount
            };
            DatabaseManager.Instance.InsertInventoryItem(newItem);
        }
    }


    public void RemoveItem(string itemId, int amount = 1)
    {
        var existing = DatabaseManager.Instance.GetInventoryItem(itemId);
        if (existing == null) return;

        existing.Amount -= amount;
        if (existing.Amount <= 0)
            DatabaseManager.Instance.DeleteInventoryItem(itemId);
        else
            DatabaseManager.Instance.UpdateInventoryItem(existing);
    }

    public bool HasItem(string itemId, int requiredAmount = 1)
    {
        var item = DatabaseManager.Instance.GetInventoryItem(itemId);
        return item != null && item.Amount >= requiredAmount;
    }

    public List<InventoryItemModel> GetAllItems()
    {
        return DatabaseManager.Instance.GetAllInventoryItems();
    }

    public bool EquipItem(string itemId)
    {
        var item = DatabaseManager.Instance.GetInventoryItem(itemId);
        if (item == null)
        {
            Debug.LogWarning($"Cannot equip item. Item not found in inventory: {itemId}");
            return false;
        }

        if (item.IsEquipped)
        {
            Debug.LogWarning($"Item is already equipped: {itemId}");
            return false;
        }

        DatabaseManager.Instance.SetItemEquippedStatus(itemId, true);
        Debug.Log($"Item equipped: {itemId}");
        return true;
    }

    public bool UnequipItem(string itemId)
    {
        var item = DatabaseManager.Instance.GetInventoryItem(itemId);
        if (item == null)
        {
            Debug.LogWarning($"Cannot unequip item. Item not found in inventory: {itemId}");
            return false;
        }

        if (!item.IsEquipped)
        {
            Debug.LogWarning($"Item is not equipped: {itemId}");
            return false;
        }

        DatabaseManager.Instance.SetItemEquippedStatus(itemId, false);
        Debug.Log($"Item unequipped: {itemId}");
        return true;
    }

    public bool IsItemEquipped(string itemId)
    {
        var item = DatabaseManager.Instance.GetInventoryItem(itemId);
        return item != null && item.IsEquipped;
    }
}
