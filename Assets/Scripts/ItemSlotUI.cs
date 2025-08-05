using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text amountText;

    public void Setup(ItemSO item, int amount)
    {
        if (!Application.isPlaying) return;
        iconImage.sprite = item.icon;
        nameText.text = item.displayName;
        amountText.text = amount.ToString();
    }
}
