using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class shopSellPanelUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public GameObject shopSellPanel;
    public CharacterStats playerStats;

    public static ItemSO currentItem;
    public static ShopItem currentShopItem;
    public static ShopData currentShop;

    public void RefreshUI()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }


        if (currentShop == null) return;

        foreach (var item in currentShop.items)
        {
            if (item == null) continue;

            ItemSO itemSO = ItemDatabase.Instance.GetItemById(item.itemId);

            if (itemSO == null) continue;

            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
            slot.GetComponent<BuyItemSlotUI>().Setup(itemSO, item.price);
        }
    }
    public static void SetCurrentShop(string ID)
    {
        currentShop = ShopManager.Instance.GetShopById(ID);
    }
    public void ToggleShopPanel()
    {
        if (shopSellPanel != null)
        {
            shopSellPanel.SetActive(!shopSellPanel.activeSelf);
        }
    }


    public void SellItems()
    {
        InventoryManager.Instance.AddItem(currentItem.itemId, 1);
        playerStats.SpendGold(currentShopItem.price);
    }

}
