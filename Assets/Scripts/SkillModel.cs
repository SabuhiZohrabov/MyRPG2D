using UnityEngine;

public enum SkillTarget
{
    Enemy,
    Ally,
    Self,
    AllySelf,
    AllEnemies,
    AllAllies
}
public enum SkillEffectType
{
    Damage,
    Heal
}

[System.Serializable]
public class SkillModel
{
    public string id; // Unique identifier for the skill
    public string name;
    public bool isPassive = false; // If true, this skill is always active and does not require activation
    public int power;
    public SkillTarget target;
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
        SkillTarget target,
        SkillEffectType effectType,
        int cooldown = 0,
        int currentCooldown = 0,
        int manaCost = 0,
        Sprite icon = null,
        string description = "")
    {
        this.name = name;
        this.power = power;
        this.target = target;
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
