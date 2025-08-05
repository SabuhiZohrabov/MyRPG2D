using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
    Quest,
    Misc
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;             // unique identifier
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Item Settings")]
    public ItemType type;
    public int maxStack = 1;
}
