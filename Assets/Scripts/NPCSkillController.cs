using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NPCSkillController : MonoBehaviour
{
    public static NPCSkillController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Execute automatic skill usage for NPC (enemy or comrade)
    public void ExecuteNPCTurn(FighterData npcFighter)
    {
        if (npcFighter == null || !npcFighter.isAlive)
            return;

        // Skip if this is a player
        if (npcFighter.isPlayer)
            return;

        // Get usable skills
        List<SkillModel> usableSkills = npcFighter.GetUsableSkills();
        
        if (usableSkills.Count == 0)
        {
            CombatLog.Instance.AddLog($"<color=orange>{npcFighter.displayName}</color> has no usable skills this turn.");
            return;
        }

        // Select skill based on AI strategy
        SkillModel selectedSkill = SelectBestSkill(npcFighter, usableSkills);
        
        if (selectedSkill != null)
        {
            // Find appropriate target
            FighterData target = SelectTarget(npcFighter, selectedSkill);
            
            if (target != null)
            {
                ExecuteSkill(npcFighter, selectedSkill, target);
            }
        }
    }

    // AI skill selection logic with class system integration
    private SkillModel SelectBestSkill(FighterData npcFighter, List<SkillModel> usableSkills)
    {
        // Get NPC class from fighter data
        NPCClass npcClass = GetNPCClass(npcFighter);
        
        // If NPC has a specific class, use class-based skill selection
        if (npcClass != null)
        {
            return npcClass.SelectSkill(npcFighter, usableSkills);
        }

        // Fallback to original logic if no class is assigned
        return SelectBestSkillDefault(npcFighter, usableSkills);
    }

    // Get NPC class from fighter data
    private NPCClass GetNPCClass(FighterData npcFighter)
    {
        // Check if it's an enemy
        if (npcFighter.fighter is EnemySO enemySO)
        {
            return enemySO.npcClass;
        }
        // Check if it's a comrade
        else if (npcFighter.fighter is ComradeData comradeData)
        {
            return comradeData.npcClass;
        }
        
        return null;
    }

    // Original skill selection logic as fallback
    private SkillModel SelectBestSkillDefault(FighterData npcFighter, List<SkillModel> usableSkills)
    {
        // Priority system for skill selection
        List<SkillModel> healingSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Heal).ToList();
        List<SkillModel> damageSkills = usableSkills.Where(s => s.effectType == SkillEffectType.Damage).ToList();

        // If NPC health is low (below 30%), prioritize healing
        float healthPercentage = (float)npcFighter.currentHP / npcFighter.maxHP;
        if (healthPercentage < 0.3f && healingSkills.Count > 0)
        {
            return healingSkills[Random.Range(0, healingSkills.Count)];
        }

        // If ally health is low, allied NPCs should prioritize healing allies
        if (npcFighter.fighter.GetFactionType() == FactionType.Allied && healingSkills.Count > 0)
        {
            FighterData lowHealthAlly = FindLowHealthAlly(npcFighter);
            if (lowHealthAlly != null)
            {
                return healingSkills[Random.Range(0, healingSkills.Count)];
            }
        }

        // Otherwise, use damage skills
        if (damageSkills.Count > 0)
        {
            return damageSkills[Random.Range(0, damageSkills.Count)];
        }

        // Fallback: random skill
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }

    // Find ally with low health for healing priority
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

    // Select appropriate target based on skill type and NPC alignment with class system integration
    private FighterData SelectTarget(FighterData npcFighter, SkillModel skill)
    {
        List<FighterData> potentialTargets = new List<FighterData>();
        FactionType enemyFaction = npcFighter.fighter.GetEnemyFaction();
        FactionType allyFaction = npcFighter.fighter.GetAllyFaction();

        switch (skill.target)
        {
            case SkillTarget.Enemy:
                // Select enemy targets based on faction
                potentialTargets = TurnManager.Instance.fighterDataList
                    .Where(f => f.isAlive && f.fighter.GetFactionType() == enemyFaction).ToList();
                break;

            case SkillTarget.Ally:
            case SkillTarget.AllySelf:
                // Select ally targets based on faction
                potentialTargets = TurnManager.Instance.fighterDataList
                    .Where(f => f.isAlive && f.fighter.GetFactionType() == allyFaction).ToList();
                break;

            case SkillTarget.Self:
                return npcFighter;

            case SkillTarget.AllEnemies:
            case SkillTarget.AllAllies:
                // For AoE skills, return the first valid target (the skill will handle multiple targets)
                return SelectTarget(npcFighter, new SkillModel("temp", 0, 
                    skill.target == SkillTarget.AllEnemies ? SkillTarget.Enemy : SkillTarget.Ally, 
                    skill.effectType));
        }

        if (potentialTargets.Count == 0)
            return null;

        // Check if NPC has a specific class for advanced targeting
        NPCClass npcClass = GetNPCClass(npcFighter);
        if (npcClass != null)
        {
            return npcClass.SelectTarget(npcFighter, skill, potentialTargets);
        }

        // Fallback to default targeting logic
        return SelectTargetDefault(skill, potentialTargets);
    }

    // Default target selection logic
    private FighterData SelectTargetDefault(SkillModel skill, List<FighterData> potentialTargets)
    {
        // Smart target selection
        if (skill.effectType == SkillEffectType.Heal)
        {
            // For healing, target the ally with lowest health percentage
            return potentialTargets.OrderBy(f => (float)f.currentHP / f.maxHP).First();
        }
        else if (skill.effectType == SkillEffectType.Damage)
        {
            // For damage, target the enemy with lowest health (finish off strategy)
            return potentialTargets.OrderBy(f => f.currentHP).First();
        }

        // Default: random target
        return potentialTargets[Random.Range(0, potentialTargets.Count)];
    }

    // Execute the selected skill on the target
    private void ExecuteSkill(FighterData npcFighter, SkillModel skill, FighterData target)
    {
        // Apply skill effect based on type
        switch (skill.effectType)
        {
            case SkillEffectType.Damage:
                if (skill.target == SkillTarget.AllEnemies)
                {
                    ExecuteAoEDamage(npcFighter, skill);
                }
                else
                {
                    target.TakeDamage(skill.power);
                    CombatLog.Instance.AddLog($"<color=orange>{npcFighter.displayName}</color> used <color=yellow>{skill.name}</color> on <color=red>{target.displayName}</color> for <b>{skill.power}</b> damage.");
                    
                    if (!target.isAlive)
                    {
                        CombatLog.Instance.AddLog($"<color=red>{target.displayName}</color> was <b>defeated</b>!");
                    }
                }
                break;

            case SkillEffectType.Heal:
                if (skill.target == SkillTarget.AllAllies)
                {
                    ExecuteAoEHeal(npcFighter, skill);
                }
                else
                {
                    target.Heal(skill.power);
                    CombatLog.Instance.AddLog($"<color=orange>{npcFighter.displayName}</color> used <color=green>{skill.name}</color> on <color=cyan>{target.displayName}</color> for <b>{skill.power}</b> HP.");
                }
                break;
        }

        // Apply costs and cooldowns
        npcFighter.UseMana(skill.manaCost);
        npcFighter.SetSkillCooldown(skill);

        // Trigger class-specific behavior if available
        NPCClass npcClass = GetNPCClass(npcFighter);
        if (npcClass != null)
        {
            npcClass.OnSkillUsed(npcFighter, skill, target);
        }

        // Refresh UI
        RefreshCombatUI();
    }

    // Execute AoE damage skill
    private void ExecuteAoEDamage(FighterData npcFighter, SkillModel skill)
    {
        List<FighterData> targets = new List<FighterData>();
        FactionType enemyFaction = npcFighter.fighter.GetEnemyFaction();
        
        // Target all enemies based on faction
        targets = TurnManager.Instance.fighterDataList
            .Where(f => f.isAlive && f.fighter.GetFactionType() == enemyFaction).ToList();

        CombatLog.Instance.AddLog($"<color=orange>{npcFighter.displayName}</color> used <color=yellow>{skill.name}</color> hitting all enemies!");

        foreach (FighterData target in targets)
        {
            target.TakeDamage(skill.power);
            CombatLog.Instance.AddLog($" - <color=red>{target.displayName}</color> took <b>{skill.power}</b> damage.");
        }
    }

    // Execute AoE heal skill
    private void ExecuteAoEHeal(FighterData npcFighter, SkillModel skill)
    {
        List<FighterData> targets = new List<FighterData>();
        FactionType allyFaction = npcFighter.fighter.GetAllyFaction();
        
        // Target all allies based on faction
        targets = TurnManager.Instance.fighterDataList
            .Where(f => f.isAlive && f.fighter.GetFactionType() == allyFaction).ToList();

        CombatLog.Instance.AddLog($"<color=orange>{npcFighter.displayName}</color> used <color=green>{skill.name}</color> healing all allies!");

        foreach (FighterData target in targets)
        {
            target.Heal(skill.power);
            CombatLog.Instance.AddLog($" - <color=cyan>{target.displayName}</color> healed for <b>{skill.power}</b> HP.");
        }
    }

    // Refresh combat UI after skill execution
    private void RefreshCombatUI()
    {
        // Refresh all fighter UIs
        for (int i = 0; i < TurnManager.Instance.fighterUIList.Count; i++)
        {
            GameObject fighterUI = TurnManager.Instance.fighterUIList[i];
            FighterUI uiComponent = fighterUI.GetComponent<FighterUI>();
            if (uiComponent != null)
            {
                uiComponent.Refresh();
            }
        }

        // Refresh skill buttons if player is active
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.RefreshSkillButtons();
        }
    }
}