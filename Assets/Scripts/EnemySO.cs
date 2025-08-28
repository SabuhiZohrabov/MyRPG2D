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
    public List<string> availableSkills = new List<string>();
    
    [Header("Skill Database")]
    public SkillDatabase skillDatabase;
    
    [Header("NPC Class")]
    public NPCClass npcClass;

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
    public List<SkillModel> AvailableSkills 
    { 
        get 
        { 
            List<SkillModel> skills = new List<SkillModel>();
            if (skillDatabase != null && availableSkills != null)
            {
                foreach (string skillId in availableSkills)
                {
                    SkillModel skill = skillDatabase.GetSkillById(skillId);
                    if (skill != null)
                        skills.Add(skill);
                }
            }
            return skills;
        } 
    }

    // Faction system methods for targeting
    public FactionType GetFactionType()
    {
        return FactionType.Enemy;
    }

    public FactionType GetEnemyFaction()
    {
        return FactionType.Allied;
    }

    public FactionType GetAllyFaction()
    {
        return FactionType.Enemy;
    }
}
