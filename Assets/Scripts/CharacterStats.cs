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
public class CharacterStats : MonoBehaviour
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

    public string PlayerName = "Sabush";
    public Sprite AvatarSprite;

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
            return PlayerName;
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
