using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SellItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text amountText;
    public TMP_Text priceText;

    private InventoryItemModel currentItem;


    public void Setup(InventoryItemModel item)
    {
        if (!Application.isPlaying) return;
        ItemSO itemSO = ItemDatabase.Instance.GetItemById(item.ItemId);
        if (itemSO == null) return;

        currentItem = item;
        iconImage.sprite = itemSO.icon;
        priceText.text = itemSO.price.ToString();
    }

}
