using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilUI : MonoBehaviour
{
    public Image avatarImage;
    public TMP_Text nameText;
    public CharacterStats characterStats;

    private void Start()
    {
        if (!Application.isPlaying) return;

        // Show avatar and name
        if (avatarImage != null && characterStats != null)
        {
            if (characterStats.icon != null)
            {
                avatarImage.sprite = characterStats.icon;
                avatarImage.color = Color.white;
                avatarImage.preserveAspect = true; // Maintain aspect ratio
            }
            else
            {
                avatarImage.color = new Color(1, 1, 1, 0.2f); // Semi-transparent if no avatar
            }
        }
        if (nameText != null && characterStats != null)
        {
            nameText.text = characterStats.DisplayName;
        }
    }

}
