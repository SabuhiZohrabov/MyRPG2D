using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "AEnemyClass", menuName = "Combat/NPC Classes/AEnemy")]
public class NPCClassAEnemy : NPCClass
{
    [Header("AEnemy Class Settings")]
    public float aggressiveHealthThreshold = 0.5f;
    public float desperateHealthThreshold = 0.2f;
    
    [Header("Advanced Tactics")]
    public bool preferAoESkills = true;
    public bool focusWeakestTarget = false;
    public bool useDefensiveSkillsWhenLow = true;

    public override SkillModel SelectSkill(FighterData npcFighter, List<SkillModel> usableSkills)
    {
        float healthPercentage = (float)npcFighter.currentHP / npcFighter.maxHP;
        
        // Categorize available skills
        List<SkillModel> healingSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Heal).ToList();
        List<SkillModel> damageSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Damage).ToList();
        List<SkillModel> aoeSkills = damageSkills.Where(s => s.target == SkillTarget.AllEnemies).ToList();
        List<SkillModel> singleTargetSkills = damageSkills.Where(s => s.target == SkillTarget.Enemy).ToList();

        // Desperate mode: below 20% health
        if (healthPercentage < desperateHealthThreshold)
        {
            // Prioritize healing or highest damage skills
            if (healingSkills.Count > 0 && useDefensiveSkillsWhenLow)
            {
                return healingSkills.OrderByDescending(s => s.power).First();
            }
            else if (damageSkills.Count > 0)
            {
                // Use most powerful damage skill available
                return damageSkills.OrderByDescending(s => s.power).First();
            }
        }

        // Aggressive mode: below 50% health but above 20%
        if (healthPercentage < aggressiveHealthThreshold)
        {
            // Prefer AoE skills if available and multiple targets exist
            if (preferAoESkills && aoeSkills.Count > 0)
            {
                List<FighterData> enemyTargets = GetPotentialTargets(npcFighter, aoeSkills[0])
                    .FindAll(f => f.fighter.GetFactionType() == npcFighter.fighter.GetEnemyFaction());
                
                if (enemyTargets.Count > 1) // Multiple enemies to hit
                {
                    return aoeSkills.OrderByDescending(s => s.power).First();
                }
            }
            
            // Use strong single target skills
            if (singleTargetSkills.Count > 0)
            {
                return singleTargetSkills.OrderByDescending(s => s.power).First();
            }
        }

        // Normal mode: above 50% health
        // Smart skill selection based on battlefield situation
        if (aoeSkills.Count > 0 && preferAoESkills)
        {
            List<FighterData> enemyTargets = GetPotentialTargets(npcFighter, aoeSkills[0])
                .FindAll(f => f.fighter.GetFactionType() == npcFighter.fighter.GetEnemyFaction());
            
            if (enemyTargets.Count > 1)
            {
                return aoeSkills[Random.Range(0, aoeSkills.Count)];
            }
        }

        // Default to damage skills
        if (damageSkills.Count > 0)
        {
            return damageSkills[Random.Range(0, damageSkills.Count)];
        }

        // Fallback
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }

    public override FighterData SelectTarget(FighterData npcFighter, SkillModel skill, List<FighterData> potentialTargets)
    {
        if (potentialTargets.Count == 0)
            return null;

        // Advanced targeting based on skill type and class settings
        if (skill.effectType == SkillEffectType.Heal)
        {
            // Target self or ally with lowest health percentage
            return potentialTargets.OrderBy(f => (float)f.currentHP / f.maxHP).First();
        }
        else if (skill.effectType == SkillEffectType.Damage)
        {
            if (focusWeakestTarget)
            {
                // Focus on finishing off weakest enemies
                return potentialTargets.OrderBy(f => f.currentHP).First();
            }
            else
            {
                // Target strongest enemy (highest current HP)
                return potentialTargets.OrderByDescending(f => f.currentHP).First();
            }
        }

        return potentialTargets[Random.Range(0, potentialTargets.Count)];
    }

    public override float GetHealthThreshold()
    {
        return aggressiveHealthThreshold;
    }

    public override bool ShouldPrioritizeHealing(FighterData npcFighter)
    {
        float healthPercentage = (float)npcFighter.currentHP / npcFighter.maxHP;
        return healthPercentage < desperateHealthThreshold && useDefensiveSkillsWhenLow;
    }

    public override void OnSkillUsed(FighterData npcFighter, SkillModel skill, FighterData target)
    {
        // Advanced enemy might taunt or show special behavior
        float healthPercentage = (float)npcFighter.currentHP / npcFighter.maxHP;
        
        if (healthPercentage < desperateHealthThreshold && skill.effectType == SkillEffectType.Damage)
        {
            CombatLog.Instance.AddLog($"<color=red>{npcFighter.displayName}</color> is getting desperate and fights more viciously!");
        }
        else if (skill.target == SkillTarget.AllEnemies)
        {
            CombatLog.Instance.AddLog($"<color=red>{npcFighter.displayName}</color> unleashes a devastating area attack!");
        }
    }
}