using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Combat/Enemy")]
public class EnemySO : ScriptableObject, IFighter
{
    public string enemyId;
    public string displayName;
    public Sprite icon;
    public int maxHP;
    public int maxMP;
    public int expReward = 10;
    
    [Header("Skills")]
    public List<SkillModel> availableSkills = new List<SkillModel>();

    // Simple loot system
    [System.Serializable]
    public class SimpleLoot
    {
        public ItemSO item;
        [Range(0f, 1f)] public float dropChance = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }
    public List<SimpleLoot> lootTable = new List<SimpleLoot>();
    
    // IFighter interface implementation
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public int MaxHP => maxHP;
    public int MaxMP => maxMP;
    public List<SkillModel> AvailableSkills => availableSkills;
}
