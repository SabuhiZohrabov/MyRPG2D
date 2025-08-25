using System.Collections.Generic;
using UnityEngine;

public static class SimpleAI
{
    // Simple AI decision making for enemies and comrades
    public static AIAction DecideAction(FighterData aiCharacter, List<FighterData> allFighters)
    {
        if (aiCharacter == null || !aiCharacter.isAlive)
            return new AIAction { actionType = AIActionType.DoNothing };

        // Get usable skills
        List<SkillModel> usableSkills = aiCharacter.GetUsableSkills();
        
        // If no skills available, do basic attack or skip turn
        if (usableSkills.Count == 0)
        {
            return new AIAction { actionType = AIActionType.BasicAttack, target = GetRandomEnemyTarget(aiCharacter, allFighters) };
        }

        // Simple skill selection - pick a random usable skill
        SkillModel selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
        FighterData target = SelectTargetForSkill(selectedSkill, aiCharacter, allFighters);
        
        return new AIAction 
        { 
            actionType = AIActionType.UseSkill, 
            skill = selectedSkill, 
            target = target 
        };
    }
    
    // Select appropriate target based on skill type
    private static FighterData SelectTargetForSkill(SkillModel skill, FighterData caster, List<FighterData> allFighters)
    {
        switch (skill.targetType)
        {
            case SkillTarget.Enemy:
                return GetRandomEnemyTarget(caster, allFighters);
                
            case SkillTarget.Ally:
                return GetRandomAllyTarget(caster, allFighters);
                
            case SkillTarget.Self:
                return caster;
                
            case SkillTarget.AllEnemies:
            case SkillTarget.AllAllies:
            default:
                return GetRandomEnemyTarget(caster, allFighters);
        }
    }
    
    // Get a random alive enemy target
    private static FighterData GetRandomEnemyTarget(FighterData attacker, List<FighterData> allFighters)
    {
        List<FighterData> enemies = new List<FighterData>();
        
        foreach (FighterData fighter in allFighters)
        {
            if (fighter.isAlive && IsEnemy(attacker, fighter))
            {
                enemies.Add(fighter);
            }
        }
        
        if (enemies.Count > 0)
            return enemies[Random.Range(0, enemies.Count)];
            
        return null;
    }
    
    // Get a random alive ally target (including self)
    private static FighterData GetRandomAllyTarget(FighterData caster, List<FighterData> allFighters)
    {
        List<FighterData> allies = new List<FighterData>();
        
        foreach (FighterData fighter in allFighters)
        {
            if (fighter.isAlive && IsAlly(caster, fighter))
            {
                allies.Add(fighter);
            }
        }
        
        if (allies.Count > 0)
            return allies[Random.Range(0, allies.Count)];
            
        return caster; // fallback to self
    }
    
    // Check if two fighters are enemies
    private static bool IsEnemy(FighterData fighter1, FighterData fighter2)
    {
        // Player and comrades vs enemies
        if ((fighter1.isPlayer || fighter1.isComrade) && fighter2.isEnemy) return true;
        if (fighter1.isEnemy && (fighter2.isPlayer || fighter2.isComrade)) return true;
        
        return false;
    }
    
    // Check if two fighters are allies
    private static bool IsAlly(FighterData fighter1, FighterData fighter2)
    {
        // Same fighter
        if (fighter1 == fighter2) return true;
        
        // Player and comrades are allies
        if ((fighter1.isPlayer || fighter1.isComrade) && (fighter2.isPlayer || fighter2.isComrade)) return true;
        
        // Enemies are allies with each other (for healing purposes)
        if (fighter1.isEnemy && fighter2.isEnemy) return true;
        
        return false;
    }
}

// AI Action data structure
[System.Serializable]
public class AIAction
{
    public AIActionType actionType;
    public SkillModel skill;
    public FighterData target;
}

// AI Action types
public enum AIActionType
{
    DoNothing,
    BasicAttack,
    UseSkill
}