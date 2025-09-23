using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuyItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;

    private ItemSO currentItem;


    public void Setup(ItemSO item, int price)
    {
        if (!Application.isPlaying) return;
        
        currentItem = item;
        iconImage.sprite = item.icon;
        nameText.text = item.displayName;
        priceText.text = price.ToString() + " gold";
    }

    public void OnItemClicked()
    {
        if (currentItem == null) return;

        // Trigger the panel activation event
        shopBuyPanelUI.currentItem = currentItem;
        shopBuyPanelUI.TriggerItemClick();
    }
}
