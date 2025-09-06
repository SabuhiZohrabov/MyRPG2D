using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
    Quest,
    Misc
}

public enum EquipmentType
{
    Weapon,
    Armor,
    Helm,
    Boots,
    Gloves,
    Belt,
    Ring,
    Necklace,
    Shield,
    Accessory,
    None
}

public static class EquipmentSlotLimits
{
    public static readonly Dictionary<EquipmentType, int> MaxSlots = new Dictionary<EquipmentType, int>
    {
        { EquipmentType.Weapon, 1 },
        { EquipmentType.Armor, 1 },
        { EquipmentType.Helm, 1 },
        { EquipmentType.Boots, 1 },
        { EquipmentType.Gloves, 1 },
        { EquipmentType.Belt, 1 },
        { EquipmentType.Ring, 3 },
        { EquipmentType.Necklace, 1 },
        { EquipmentType.Shield, 1 },
        { EquipmentType.Accessory, 2 },
        { EquipmentType.None, 0 }
    };

    public static int GetMaxSlots(EquipmentType equipmentType)
    {
        return MaxSlots.TryGetValue(equipmentType, out int maxSlots) ? maxSlots : 1;
    }
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
    
    [Header("Equipment Settings")]
    public EquipmentType equipmentType;
}
