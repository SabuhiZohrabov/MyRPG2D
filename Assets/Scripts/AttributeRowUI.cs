using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class AttributeRowUI : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text attributeLabel; // Displays the attribute name and value, e.g., "Strength: 5"
    public Button addButton;        // The "+" button to increase the attribute

    private string attributeName;             // Stores the attribute's internal name
    private Action<string> onAddCallback;     // External callback to invoke on button click

    private UnityAction cachedButtonAction;
    private bool isInitialized = false;

    private void Awake()
    {
        if (addButton != null)
        {
            cachedButtonAction = OnButtonClicked;
        }
    }

    /// <summary>
    /// Initializes the UI row with attribute data and button behavior.
    /// Called when the row is instantiated.
    /// </summary>
    public void Setup(string name, int value, bool canAdd, Action<string> addCallback)
    {
        attributeName = name;                 // Store which attribute this row represents
        onAddCallback = addCallback;          // Save the external callback function

        if (attributeLabel != null)
        {
            attributeLabel.text = $"{name}: {value}"; // e.g., "Strength: 5"
        }

        if (addButton != null)
        {
            addButton.interactable = canAdd;          // Enable or disable the "+" button

            if (!isInitialized)
            {
                addButton.onClick.AddListener(cachedButtonAction);
                isInitialized = true;
            }
        }
    }

    /// <summary>
    /// Updates the label and button state when the attribute value changes.
    /// </summary>
    public void UpdateValue(int value, bool canAdd)
    {
        if (attributeLabel != null)
        {
            attributeLabel.text = $"{attributeName}: {value}"; // Update label with new value
        }

        if (addButton != null)
        {
            addButton.interactable = canAdd;                   // Enable or disable the "+" button
        }
    }

    private void OnButtonClicked()
    {
        onAddCallback?.Invoke(attributeName);
    }

    private void OnDestroy()
    {
        if (addButton != null && cachedButtonAction != null)
        {
            addButton.onClick.RemoveListener(cachedButtonAction);
        }
    }
}