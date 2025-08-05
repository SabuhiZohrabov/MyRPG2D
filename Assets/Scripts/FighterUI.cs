using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FighterUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public Image hpBarFill;
    public GameObject deathOverlay; // Optional red overlay image for dead state

    [Header("Animation Settings")]
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private Color hitFlashColor = Color.red;

    private static WaitForSeconds hitFlashWait;

    private Color originalIconColor;

    private bool isAnimating = false;

    private FighterData model;

    private void Awake()
    {
        // Initialize static WaitForSeconds only once
        if (hitFlashWait == null)
        {
            hitFlashWait = new WaitForSeconds(hitFlashDuration);
        }

        // Cache original icon color if iconImage is assigned
        if (iconImage != null)
        {
            originalIconColor = iconImage.color;
        }
    }

    public void Setup(FighterData data)
    {
        model = data;

        if (nameText != null && nameText.text != model.displayName)
        {
            nameText.text = model.displayName;
        }

        UpdateHPBar();
    }

    public void Refresh()
    {
        if (model != null)
        {
            UpdateHPBar();

            if (!model.isAlive)
            {
                SetDeadVisual();
            }
        }
    }

    private void UpdateHPBar()
    {
        if (hpBarFill == null || model == null || model.maxHP <= 0)
            return;

        float newFillAmount = (float)model.currentHP / model.maxHP;

        // Only update if value has changed significantly (prevent micro-updates)
        if (Mathf.Abs(hpBarFill.fillAmount - newFillAmount) > 0.001f)
        {
            hpBarFill.fillAmount = newFillAmount;
        }
    }

    private void SetDeadVisual()
    {
        // Fade the icon to show the fighter is dead
        if (iconImage != null)
        {
            iconImage.color = new Color(originalIconColor.r, originalIconColor.g, originalIconColor.b, 0.3f);
        }

        // Enable red overlay image if assigned
        if (deathOverlay != null && !deathOverlay.activeInHierarchy)
        {
            deathOverlay.SetActive(true);
        }

        // Disable interaction with the dead fighter
        Button btn = GetComponent<Button>();
        if (btn != null && btn.interactable)
        {
            btn.interactable = false;
        }
    }

    public void PlayHitAnimation()
    {
        // Prevent overlapping animations
        if (isAnimating || iconImage == null)
            return;

        StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        isAnimating = true;

        // Store current color (might be different if fighter is dead)
        Color currentColor = iconImage.color;

        // Flash to hit color
        iconImage.color = hitFlashColor;

        //USE CACHED WaitForSeconds - NO MORE GARBAGE COLLECTION!
        yield return hitFlashWait;

        // Restore original color
        iconImage.color = currentColor;

        isAnimating = false;
    }

    private void OnDisable()
    {
        isAnimating = false;

        // Restore original color if animation was interrupted
        if (iconImage != null)
        {
            iconImage.color = model != null && !model.isAlive ?
                new Color(originalIconColor.r, originalIconColor.g, originalIconColor.b, 0.3f) :
                originalIconColor;
        }
    }

    public void ResetForPooling()
    {
        isAnimating = false;
        model = null;

        if (iconImage != null)
        {
            iconImage.color = originalIconColor;
        }

        if (deathOverlay != null)
        {
            deathOverlay.SetActive(false);
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
        }
    }
}