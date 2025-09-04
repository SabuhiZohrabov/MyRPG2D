using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text amountText;
    public Image isEquipedBG;

    public void Setup(ItemSO item, int amount, bool isEquiped)
    {
        if (!Application.isPlaying) return;
        iconImage.sprite = item.icon;
        nameText.text = item.displayName;
        amountText.text = amount.ToString();
        isEquipedBG.gameObject.SetActive(isEquiped);
    }
}
