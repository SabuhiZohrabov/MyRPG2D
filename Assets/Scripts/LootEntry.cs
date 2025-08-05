using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemSO item;
    [Range(0f, 1f)] public float dropChance = 1f;
    public int minAmount = 1;
    public int maxAmount = 1;
}
