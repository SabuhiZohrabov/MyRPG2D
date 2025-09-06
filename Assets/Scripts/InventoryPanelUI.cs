using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    private void OnEnable()
    {
        if (!Application.isPlaying) return;// Editor mode check
        RefreshUI();
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
            slot.GetComponent<ItemSlotUI>().Setup(itemSO, model);
        }
    }
}
