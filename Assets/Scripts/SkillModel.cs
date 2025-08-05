using UnityEngine;

public enum SkillTarget
{
    Enemy,
    Ally,
    Self,
    AllEnemies,
    AllAllies
}
public enum SkillEffectType
{
    Damage,
    Heal,
    AreaDamage
}

[System.Serializable]
public class SkillModel
{
    public string id; // Unique identifier for the skill
    public string name;
    public bool isPassive = false; // If true, this skill is always active and does not require activation
    public bool isLearned = true; // If true, this skill is available for use; if false, it is not learned yet
    public int power;
    public SkillTarget targetType;
    public SkillEffectType effectType;
    public int cooldown = 0;        // static cooldown duration
    public int currentCooldown = 0;      // runtime cooldown counter
    public int manaCost = 0;
    public Sprite icon;
    [TextArea]
    public string description;
    public SkillModel(
        string name,
        int power,
        SkillTarget targetType,
        SkillEffectType effectType,
        int cooldown = 0,
        int currentCooldown = 0,
        int manaCost = 0,
        Sprite icon = null,
        string description = "")
    {
        this.name = name;
        this.power = power;
        this.targetType = targetType;
        this.effectType = effectType;
        this.cooldown = cooldown;
        this.currentCooldown = currentCooldown;
        this.manaCost = manaCost;
        this.icon = icon;
        this.description = description;
    }
    public bool IsAvailable()
    {
        return currentCooldown <= 0;
    }
}
