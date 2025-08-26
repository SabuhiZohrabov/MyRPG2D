using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewComrade", menuName = "Combat/Comrade")]
public class ComradeData : ScriptableObject, IFighter
{
    [Header("Basic Info")]
    public string comradeId;
    public string displayName;
    public Sprite icon;
    
    [Header("Combat Stats")]
    public int maxHP;
    public int maxMP;
    
    [Header("Skills")]
    public List<string> availableSkills = new List<string>();
    
    [Header("Skill Database")]
    public SkillDatabase skillDatabase;
    
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
}