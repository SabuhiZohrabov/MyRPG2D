using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "RPG/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    [Header("All Available Skills")]
    public List<SkillModel> allSkills = new List<SkillModel>();

    // Get skill by ID
    public SkillModel GetSkillById(string skillId)
    {
        foreach (SkillModel skill in allSkills)
        {
            if (skill.id == skillId)
            {
                return skill;
            }
        }
        return null;
    }

    // Check if skill exists in database
    public bool HasSkill(string skillId)
    {
        return GetSkillById(skillId) != null;
    }
}