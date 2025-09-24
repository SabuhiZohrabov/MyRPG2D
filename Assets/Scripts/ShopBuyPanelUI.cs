using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class shopBuyPanelUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public GameObject ItemDescriptionPanel;
    public GameObject shopBuyPanel;
    public CharacterStats playerStats;

    public Image iconImage;
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;

    public static ItemSO currentItem;
    public static ShopItem currentShopItem;
    public static ShopData currentShop;

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
    public void CloseShopPanel()
    {
        if (shopBuyPanel != null)
        {
            shopBuyPanel.SetActive(!shopBuyPanel.activeSelf);
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
        if (currentShopItem == null) return;
        currentItem = ItemDatabase.Instance.GetItemById(currentShopItem.itemId);
        if (currentItem == null) return;
        iconImage.sprite = currentItem.icon;
        itemNameText.text = currentItem.displayName;
        itemDescText.text = currentItem.description;
    }

    public void BuyItem()
    {
        InventoryManager.Instance.AddItem(currentItem.itemId, 1);
        playerStats.SpendGold(currentShopItem.price);
    }

    /// <summary>
    /// Static method to trigger the item click event
    /// </summary>
    public static void TriggerItemClick()
    {
        OnItemClickedEvent?.Invoke();
    }
}
