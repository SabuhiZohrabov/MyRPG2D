using UnityEngine;
using System.Collections.Generic;

public abstract class NPCClass : ScriptableObject
{
    [Header("Class Info")]
    public string className;
    public string description;
    
    // Abstract methods that each class must implement
    public abstract SkillModel SelectSkill(FighterData npcFighter, List<SkillModel> usableSkills);
    public abstract FighterData SelectTarget(FighterData npcFighter, SkillModel skill, List<FighterData> potentialTargets);
    public abstract float GetHealthThreshold();
    public abstract bool ShouldPrioritizeHealing(FighterData npcFighter);
    
    // Optional method for class-specific combat behavior
    public virtual void OnSkillUsed(FighterData npcFighter, SkillModel skill, FighterData target)
    {
        // Base implementation does nothing, subclasses can override
    }
    
    // Helper method to get all potential targets based on skill type and faction
    protected List<FighterData> GetPotentialTargets(FighterData npcFighter, SkillModel skill)
    {
        List<FighterData> potentialTargets = new List<FighterData>();
        FactionType enemyFaction = npcFighter.fighter.GetEnemyFaction();
        FactionType allyFaction = npcFighter.fighter.GetAllyFaction();

        switch (skill.target)
        {
            case SkillTarget.Enemy:
                potentialTargets = TurnManager.Instance.fighterDataList
                    .FindAll(f => f.isAlive && f.fighter.GetFactionType() == enemyFaction);
                break;

            case SkillTarget.Ally:
            case SkillTarget.AllySelf:
                potentialTargets = TurnManager.Instance.fighterDataList
                    .FindAll(f => f.isAlive && f.fighter.GetFactionType() == allyFaction);
                break;

            case SkillTarget.AllEnemies:
                potentialTargets = TurnManager.Instance.fighterDataList
                    .FindAll(f => f.isAlive && f.fighter.GetFactionType() == enemyFaction);
                break;

            case SkillTarget.AllAllies:
                potentialTargets = TurnManager.Instance.fighterDataList
                    .FindAll(f => f.isAlive && f.fighter.GetFactionType() == allyFaction);
                break;
        }

        return potentialTargets;
    }
}