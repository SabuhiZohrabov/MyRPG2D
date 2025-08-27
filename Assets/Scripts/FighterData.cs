using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Faction types for targeting system

[System.Serializable]
public class FighterData
{
    // Interface reference
    public IFighter fighter;
    
    // Type identification
    public bool isPlayer;
    public bool isComrade;
    public bool isEnemy;

    // Dynamic runtime values
    public int currentHP;
    public int currentMP;
    public bool isAlive = true;
    
    // Skill cooldown tracking - key: skill id, value: current cooldown
    private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();

    // --- Universal properties (always use these instead of direct fields!) ---

    public string displayName
    {
        get
        {
            return fighter?.DisplayName ?? "Unknown";
        }
    }

    public int maxHP
    {
        get
        {
            return fighter?.MaxHP ?? 100;
        }
    }

    public int maxMP
    {
        get
        {
            return fighter?.MaxMP ?? 50;
        }
    }

    // --- Constructors ---

    // Enemy constructor
    public FighterData(EnemySO enemy)
    {
        fighter = enemy;
        isPlayer = false;
        isComrade = false;
        isEnemy = true;
        currentHP = enemy?.MaxHP ?? 100;
        currentMP = enemy?.MaxMP ?? 50;
        isAlive = true;
    }

    // Player constructor
    public FighterData(CharacterStats stats)
    {
        fighter = stats;
        isPlayer = true;
        isComrade = false;
        isEnemy = false;
        currentHP = stats?.MaxHP ?? 100;
        currentMP = stats?.MaxMP ?? 50;
        isAlive = true;
    }

    // Comrade constructor
    public FighterData(ComradeData comrade)
    {
        fighter = comrade;
        isPlayer = false;
        isComrade = true;
        isEnemy = false;
        currentHP = comrade?.MaxHP ?? 100;
        currentMP = comrade?.MaxMP ?? 50;
        isAlive = true;
    }

    //// --- Faction system for targeting ---

    //// Get this fighter's faction type
    //public virtual FactionType GetFactionType()
    //{
    //    if (isEnemy) return FactionType.Enemy;
    //    return FactionType.Allied; // Player + Comrade
    //}

    //// Get enemy faction to target for damage skills
    //public virtual FactionType GetEnemyFaction()
    //{
    //    return GetFactionType() == FactionType.Allied ? FactionType.Enemy : FactionType.Allied;
    //}

    //// Get ally faction to target for heal skills  
    //public virtual FactionType GetAllyFaction()
    //{
    //    return GetFactionType();
    //}

    // --- Universal methods ---

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0)
            currentHP = 0;
        if (currentHP == 0)
            isAlive = false;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMP >= amount;
    }

    public void UseMana(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0)
            currentMP = 0;
    }

    // --- Skill Methods ---
    
    // Get available skills from the underlying fighter
    public List<SkillModel> GetAvailableSkills()
    {
        return fighter?.AvailableSkills ?? new List<SkillModel>();
    }
    
    // Get usable skills (learned, not on cooldown, enough mana)
    public List<SkillModel> GetUsableSkills()
    {
        List<SkillModel> usableSkills = new List<SkillModel>();
        foreach (SkillModel skill in GetAvailableSkills())
        {
            if (!skill.isPassive && IsSkillAvailable(skill) && HasEnoughMana(skill.manaCost))
            {
                usableSkills.Add(skill);
            }
        }
        return usableSkills;
    }
    
    // Use a random skill from available skills (for AI)
    public SkillModel GetRandomUsableSkill()
    {
        List<SkillModel> usableSkills = GetUsableSkills();
        if (usableSkills.Count > 0)
        {
            return usableSkills[UnityEngine.Random.Range(0, usableSkills.Count)];
        }
        return null;
    }
    
    // Check if this fighter can use a specific skill
    public bool CanUseSkill(SkillModel skill)
    {
        if (skill == null) return false;
        return !skill.isPassive && IsSkillAvailable(skill) && HasEnoughMana(skill.manaCost);
    }
    
    // --- Skill Cooldown Management ---
    
    // Check if a skill is available (not on cooldown)
    public bool IsSkillAvailable(SkillModel skill)
    {
        if (skill == null || string.IsNullOrEmpty(skill.id)) return false;
        return GetSkillCooldown(skill.id) <= 0;
    }
    
    // Get current cooldown for a skill
    public int GetSkillCooldown(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return 0;
        return skillCooldowns.ContainsKey(skillId) ? skillCooldowns[skillId] : 0;
    }
    
    // Set skill on cooldown
    public void SetSkillCooldown(SkillModel skill)
    {
        if (skill == null || string.IsNullOrEmpty(skill.id)) return;
        skillCooldowns[skill.id] = skill.cooldown;
    }
    
    // Reduce all skill cooldowns by 1 (called at end of turn)
    public void ReduceSkillCooldowns()
    {
        var keys = skillCooldowns.Keys.ToList();
        foreach (string skillId in keys)
        {
            if (skillCooldowns[skillId] > 0)
            {
                skillCooldowns[skillId]--;
            }
        }
    }
}
