using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyLoot", menuName = "Combat/Enemy Loot Table")]
public class EnemyLootSO : ScriptableObject
{
    public List<LootEntry> lootEntries = new List<LootEntry>();
}
