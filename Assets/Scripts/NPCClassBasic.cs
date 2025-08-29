using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "BasicNPCClass", menuName = "Combat/NPC Classes/Basic")]
public class NPCClassBasic : NPCClass
{
    [Header("Basic Class Settings")]
    public float lowHealthThreshold = 0.3f;
    
    public override SkillModel SelectSkill(FighterData npcFighter, List<SkillModel> usableSkills)
    {
        // Simple priority system: heal when low health, otherwise attack
        List<SkillModel> healingSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Heal).ToList();
        List<SkillModel> damageSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Damage).ToList();

        // Check if NPC needs healing
        if (ShouldPrioritizeHealing(npcFighter) && healingSkills.Count > 0)
        {
            return healingSkills[Random.Range(0, healingSkills.Count)];
        }

        // Check if ally needs healing (for allied NPCs)
        if (npcFighter.fighter.GetFactionType() == FactionType.Allied && healingSkills.Count > 0)
        {
            FighterData lowHealthAlly = FindLowHealthAlly(npcFighter);
            if (lowHealthAlly != null)
            {
                return healingSkills[Random.Range(0, healingSkills.Count)];
            }
        }

        // Default to damage skills
        if (damageSkills.Count > 0)
        {
            return damageSkills[Random.Range(0, damageSkills.Count)];
        }

        // Fallback: random skill
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }

    public override FighterData SelectTarget(FighterData npcFighter, SkillModel skill, List<FighterData> potentialTargets)
    {
        if (potentialTargets.Count == 0)
            return null;

        // Basic targeting logic
        if (skill.effectType == SkillEffectType.Heal)
        {
            // Target ally with lowest health percentage
            return potentialTargets.OrderBy(f => (float)f.currentHP / f.maxHP).First();
        }
        else if (skill.effectType == SkillEffectType.Damage)
        {
            // Target enemy with lowest health (finish off strategy)
            return potentialTargets.OrderBy(f => f.currentHP).First();
        }

        // Default: random target
        return potentialTargets[Random.Range(0, potentialTargets.Count)];
    }

    public override float GetHealthThreshold()
    {
        return lowHealthThreshold;
    }

    public override bool ShouldPrioritizeHealing(FighterData npcFighter)
    {
        float healthPercentage = (float)npcFighter.currentHP / npcFighter.maxHP;
        return healthPercentage < lowHealthThreshold;
    }

    private FighterData FindLowHealthAlly(FighterData npcFighter)
    {
        List<FighterData> allies = new List<FighterData>();
        FactionType allyFaction = npcFighter.fighter.GetAllyFaction();
        
        foreach (FighterData fighter in TurnManager.Instance.fighterDataList)
        {
            if (fighter.isAlive && fighter.fighter.GetFactionType() == allyFaction)
            {
                float healthPercentage = (float)fighter.currentHP / fighter.maxHP;
                if (healthPercentage < 0.5f) // Below 50% health
                {
                    allies.Add(fighter);
                }
            }
        }

        return allies.Count > 0 ? allies.OrderBy(f => f.currentHP).First() : null;
    }
}