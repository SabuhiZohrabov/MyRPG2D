using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillRowUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descText;

    public void Setup(SkillModel skill)
    {
        if (iconImage != null) iconImage.sprite = skill.icon;
        if (nameText != null) nameText.text = skill.name;
        if (descText != null) descText.text = skill.description;
    }
}
