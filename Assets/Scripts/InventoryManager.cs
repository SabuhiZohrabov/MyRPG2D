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


    public void RemoveItem(int Id, int amount = 1)
    {
        var existing = DatabaseManager.Instance.GetInventoryItemByID(Id);
        if (existing == null) return;

        existing.Amount -= amount;
        if (existing.Amount <= 0)
            DatabaseManager.Instance.DeleteInventoryItem(existing.Id);
        else
            DatabaseManager.Instance.UpdateInventoryItem(existing);
    }

    public List<InventoryItemModel> GetAllItems()
    {
        return DatabaseManager.Instance.GetAllInventoryItems();
    }

    public bool EquipItem(int Id)
    {
        var item = DatabaseManager.Instance.GetInventoryItemByID(Id);
        if (item == null)
        {
            Debug.LogWarning($"Cannot equip item. Item not found in inventory: {Id}");
            return false;
        }

        if (item.IsEquipped)
        {
            Debug.LogWarning($"Item is already equipped: {item.ItemId}");
            return false;
        }

        ItemSO itemSO = ItemDatabase.Instance.GetItemById(item.ItemId);
        if (itemSO == null || itemSO.type != ItemType.Equipment || itemSO.equipmentType == EquipmentType.None)
        {
            Debug.LogWarning($"Item cannot be equipped: {item.ItemId}");
            return false;
        }

        // Check slot limits and handle replacement if necessary
        var currentEquippedItems = GetEquippedItemsByType(itemSO.equipmentType);
        int maxSlots = EquipmentSlotLimits.GetMaxSlots(itemSO.equipmentType);
        
        if (currentEquippedItems.Count >= maxSlots)
        {
            // Randomly unequip one of the currently equipped items
            //var randomIndex = UnityEngine.Random.Range(0, currentEquippedItems.Count);
            var itemToUnequip = currentEquippedItems[0];
            DatabaseManager.Instance.SetItemEquippedStatus(itemToUnequip, false);
            Debug.Log($"Auto-unequipped {itemToUnequip.ItemId} to make space for {item.ItemId}");
        }

        DatabaseManager.Instance.SetItemEquippedStatus(item, true);
        Debug.Log($"Item equipped: {item.ItemId}");
        return true;
    }

    public bool UnequipItem(int Id)
    {
        var item = DatabaseManager.Instance.GetInventoryItemByID(Id);
        if (item == null)
        {
            Debug.LogWarning($"Cannot unequip item. Item not found in inventory: {Id}");
            return false;
        }

        if (!item.IsEquipped)
        {
            Debug.LogWarning($"Item is not equipped: {item.ItemId}");
            return false;
        }

        DatabaseManager.Instance.SetItemEquippedStatus(item, false);
        Debug.Log($"Item unequipped: {item.ItemId}");
        return true;
    }

    public bool IsItemEquipped(int Id)
    {
        var item = DatabaseManager.Instance.GetInventoryItemByID(Id);
        return item != null && item.IsEquipped;
    }

    public List<InventoryItemModel> GetEquippedItemsByType(EquipmentType equipmentType)
    {
        var allItems = GetAllItems();
        var equippedItems = new List<InventoryItemModel>();

        foreach (var item in allItems)
        {
            if (item.IsEquipped)
            {
                ItemSO itemSO = ItemDatabase.Instance.GetItemById(item.ItemId);
                if (itemSO != null && itemSO.equipmentType == equipmentType)
                {
                    equippedItems.Add(item);
                }
            }
        }

        return equippedItems;
    }

    public int GetEquippedItemCount(EquipmentType equipmentType)
    {
        return GetEquippedItemsByType(equipmentType).Count;
    }

    public bool CanEquipItem(string itemId)
    {
        ItemSO itemSO = ItemDatabase.Instance.GetItemById(itemId);
        if (itemSO == null || itemSO.type != ItemType.Equipment || itemSO.equipmentType == EquipmentType.None)
        {
            return false;
        }

        int currentEquipped = GetEquippedItemCount(itemSO.equipmentType);
        int maxSlots = EquipmentSlotLimits.GetMaxSlots(itemSO.equipmentType);

        return currentEquipped < maxSlots || maxSlots > 0;
    }
}
