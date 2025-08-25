using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeUpgradeType
{
    Manual,
    Auto,
    ManualAndAuto
}
[System.Serializable]
public class CharacterStats : MonoBehaviour, IFighter
{
    public Attribute Strength = new Attribute(0);
    public Attribute Dexterity = new Attribute(0);
    public Attribute Intelligence = new Attribute(0);
    public Attribute Endurance = new Attribute(0);

    public int Level = 1;
    public int CurrentXP = 0;
    public int XPToNextLevel = 1000;
    public int AvailableAttributePoints = 0;
    public int PointGainedPerLevel = 3;
    public int maxHP = 1000;
    public int maxMP = 1000;
    public string CurrentAdventureId = "";

    public string displayName = "Sabush";
    public Sprite icon;
    
    [Header("Player Skills")]
    public List<SkillModel> playerSkills = new List<SkillModel>();
    
    [Header("Skill Database")]
    public SkillDatabase skillDatabase;

    private int databaseId = 0;

    public void SetDatabaseId(int id)
    {
        databaseId = id;
    }

    public int MaxHP
    {
        get
        {
            return Endurance.Value * 5 + maxHP;
        }
    }

    public int MaxMP
    {
        get
        {
            return Intelligence.Value * 5 + maxMP;
        }
    }

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }
    
    // IFighter interface implementation
    public Sprite Icon => icon;
    
    // Skills - return player's own skill list
    public List<SkillModel> AvailableSkills 
    { 
        get 
        { 
            return playerSkills ?? new List<SkillModel>();
        } 
    }

    private Dictionary<string, AttributeUpgradeType> attributeUpgradeRules = new Dictionary<string, AttributeUpgradeType>
{
    { "Strength", AttributeUpgradeType.Manual },
    { "Dexterity", AttributeUpgradeType.Manual },
    { "Intelligence", AttributeUpgradeType.Manual },
    { "Endurance", AttributeUpgradeType.ManualAndAuto }
};
    private void Start()
    {
        if (!Application.isPlaying) return;
        DatabaseManager.LoadStatsToCharacter(this);
    }
    public Dictionary<string, Attribute> GetAllAttributes()
    {
        return new Dictionary<string, Attribute>
        {
            { "Strength", Strength },
            { "Dexterity", Dexterity },
            { "Intelligence", Intelligence },
            { "Endurance", Endurance }
        };
    }

    public bool IncreaseAttribute(string attributeName)
    {
        //Debug.Log($"Trying to increase: {attributeName}, Available Points: {AvailableAttributePoints}");

        if (AvailableAttributePoints <= 0) return false;
        if (!attributeUpgradeRules.ContainsKey(attributeName)) return false;
        
        var type = attributeUpgradeRules[attributeName];
        if (type != AttributeUpgradeType.Manual && type != AttributeUpgradeType.ManualAndAuto) return false;

        var attributes = GetAllAttributes();
        if (!attributes.ContainsKey(attributeName)) return false;

        attributes[attributeName].Value += 1;
        AvailableAttributePoints--;
        SaveToDatabase();
        return true;
    }

    public void LevelUp(int pointsToAdd = 3)
    {
        var attributes = GetAllAttributes();

        foreach (var pair in attributeUpgradeRules)
        {
            if ((pair.Value == AttributeUpgradeType.Auto || pair.Value == AttributeUpgradeType.ManualAndAuto)
                && attributes.ContainsKey(pair.Key))
            {
                attributes[pair.Key].Value += 1;
            }
        }

        AvailableAttributePoints += pointsToAdd;
        SaveToDatabase();
    }
    public bool CanIncrease(string attributeName)
    {
        if (AvailableAttributePoints <= 0) return false;
        if (!attributeUpgradeRules.ContainsKey(attributeName)) return false;

        var type = attributeUpgradeRules[attributeName];
        return (type == AttributeUpgradeType.Manual || type == AttributeUpgradeType.ManualAndAuto);
    }
    public void GainXP(int amount)
    {
        CurrentXP += amount;

        while (CurrentXP >= XPToNextLevel)
        {
            CurrentXP -= XPToNextLevel;
            Level++;
            AvailableAttributePoints += PointGainedPerLevel;

            foreach (var pair in attributeUpgradeRules)
            {
                if ((pair.Value == AttributeUpgradeType.Auto || pair.Value == AttributeUpgradeType.ManualAndAuto)
                    && GetAllAttributes().ContainsKey(pair.Key))
                {
                    GetAllAttributes()[pair.Key].Value += 1;
                }
            }
                        
            XPToNextLevel = Mathf.RoundToInt(XPToNextLevel * 1.2f);
        }
        SaveToDatabase();
    }
    // Skill management methods
    public bool AddSkill(string skillId)
    {
        if (skillDatabase == null)
        {
            Debug.LogError("SkillDatabase is not assigned!");
            return false;
        }
        
        // Check if skill already exists in player skills
        if (HasSkill(skillId))
        {
            Debug.LogWarning($"Player already has skill: {skillId}");
            return false;
        }
        
        // Get skill from database
        SkillModel skillToAdd = skillDatabase.GetSkillById(skillId);
        if (skillToAdd == null)
        {
            Debug.LogWarning($"Skill with ID {skillId} not found in database!");
            return false;
        }
        
        // Create a copy of the skill and add to player skills
        SkillModel playerSkill = new SkillModel(
            skillToAdd.name,
            skillToAdd.power,
            skillToAdd.targetType,
            skillToAdd.effectType,
            skillToAdd.cooldown,
            skillToAdd.currentCooldown,
            skillToAdd.manaCost,
            skillToAdd.icon,
            skillToAdd.description
        );
        playerSkill.id = skillToAdd.id;
        playerSkill.isPassive = skillToAdd.isPassive;
        playerSkill.isLearned = skillToAdd.isLearned;
        
        playerSkills.Add(playerSkill);
        SaveToDatabase();
        Debug.Log($"Added skill: {skillToAdd.name} to player");
        return true;
    }
    
    public bool RemoveSkill(string skillId)
    {
        SkillModel skillToRemove = playerSkills.Find(skill => skill.id == skillId);
        if (skillToRemove == null)
        {
            Debug.LogWarning($"Player doesn't have skill with ID: {skillId}");
            return false;
        }
        
        playerSkills.Remove(skillToRemove);
        SaveToDatabase();
        Debug.Log($"Removed skill: {skillToRemove.name} from player");
        return true;
    }
    
    public bool HasSkill(string skillId)
    {
        return playerSkills.Find(skill => skill.id == skillId) != null;
    }

    public void SaveToDatabase()
    {
        PlayerStatsModel data = new PlayerStatsModel
        {
            Id = databaseId,
            Level = this.Level,
            CurrentXP = this.CurrentXP,
            XPToNextLevel = this.XPToNextLevel,
            AvailableAttributePoints = this.AvailableAttributePoints,
            Strength = this.Strength.Value,
            Dexterity = this.Dexterity.Value,
            Intelligence = this.Intelligence.Value,
            Endurance = this.Endurance.Value,
            CurrentAdventureId = this.CurrentAdventureId ?? "start_adventure"
        };

        DatabaseManager.Instance.SavePlayerStats(data);
    }

}
