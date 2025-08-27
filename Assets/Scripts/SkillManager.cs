using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject skillButtonPrefab;
    public Transform skillContainer;
    public ScrollRect scrollRect;

    private SkillModel selectedSkill;
    private bool isTargetSelectionActive = false;

    [SerializeField]
    private CharacterStats playerStats;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {        
        PopulateSkills();
    }

    void PopulateSkills()
    {
        if (playerStats == null) return;
        
        List<SkillModel> filteredSkills = playerStats.AvailableSkills.FindAll(s => !s.isPassive);
        foreach (SkillModel skill in filteredSkills)
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillContainer);

            // Set name text
            TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Image skillIcon = buttonObj.transform.Find("SkillImage")?.GetComponentInChildren<Image>();
            if (nameText != null)
                nameText.text = skill.name;
            if (skillIcon != null)
                skillIcon.sprite = skill.icon;

            // Optional: set icon here if you add Image component reference

            // Store reference for later if needed (optional)
            buttonObj.name = skill.name;

            // Add OnClick listener
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSkillClicked(skill));
            }
        }
        scrollRect.horizontalNormalizedPosition = 0f;
    }

    void OnSkillClicked(SkillModel skill)
    {
        FighterData activeFighter = TurnManager.Instance.GetCurrentFighterModel();

        if (!activeFighter.IsSkillAvailable(skill))
        {
            CombatLog.Instance.AddLog($"{skill.name} is on cooldown.");
            return;
        }

        if (!activeFighter.HasEnoughMana(skill.manaCost))
        {
            CombatLog.Instance.AddLog($"Not enough mana to use <color=yellow>{skill.name}</color>.");
            return;
        }

        selectedSkill = skill;
        isTargetSelectionActive = true;
        Debug.Log($"Select a target for {skill.name}");
    }

    public void OnTargetSelected(GameObject targetGO)
    {
        if (!isTargetSelectionActive || selectedSkill == null) return;

        ApplySkillEffect(targetGO);

        isTargetSelectionActive = false;
        selectedSkill = null;
    }
    private void ApplySkillEffect(GameObject targetGO)
    {
        int index = TurnManager.Instance.GetFighterIndex(targetGO);
        if (index == -1) return;
        FighterData targetModel = TurnManager.Instance.GetFighterModel(index);
        FighterData activeFighter = TurnManager.Instance.GetCurrentFighterModel();

        switch (selectedSkill.effectType)
        {
            case SkillEffectType.Damage:
                if (selectedSkill.target is SkillTarget.Enemy)
                {
                    if (targetModel.isAlive)
                    {
                        targetModel.TakeDamage(selectedSkill.power);
                        CombatLog.Instance.AddLog($"<color=yellow>{selectedSkill.name}</color> hit <color=red>{targetModel.displayName}</color> for <b>{selectedSkill.power}</b> damage.");
                        if (!targetModel.isAlive)
                        {
                            CombatLog.Instance.AddLog($"<color=red>{targetModel.displayName}</color> was <b>defeated</b>!");
                        }
                    } 
                }
                else
                {
                    CombatLog.Instance.AddLog($"<color=orange>{selectedSkill.name}</color> hit all enemies!");

                    for (int i = 0; i < TurnManager.Instance.fighterDataList.Count; i++)
                    {
                        var m = TurnManager.Instance.fighterDataList[i];
                        if (!m.isPlayer && m.isAlive)
                        {
                            m.TakeDamage(selectedSkill.power);
                            CombatLog.Instance.AddLog($" - <color=red>{m.displayName}</color> took <b>{selectedSkill.power}</b> damage.");

                            // UI refresh
                            var go = TurnManager.Instance.fighterUIList[i];
                            var ui = go.GetComponent<FighterUI>();
                            if (ui != null)
                                ui.Refresh();
                        }
                    }
                }
                break;

            case SkillEffectType.Heal:
                targetModel.Heal(selectedSkill.power);
                CombatLog.Instance.AddLog($"<color=green>{selectedSkill.name}</color> healed <color=#00FFFF>{targetModel.displayName}</color> for <b>{selectedSkill.power}</b> HP.");
                break;
        }

        // Refresh target UI
        FighterUI uiTarget = targetGO.GetComponent<FighterUI>();
        if (uiTarget != null)
        {
            uiTarget.Refresh();
            uiTarget.PlayHitAnimation();
        }
        //mana
        activeFighter.UseMana(selectedSkill.manaCost);
        //cooldown
        activeFighter.SetSkillCooldown(selectedSkill);

        // check victory
        var turnManager = TurnManager.Instance;
        if (turnManager != null && !turnManager.IsEnemyTeamAlive() && turnManager.IsPlayerTeamAlive())
            turnManager.OnVictory();

        // Refresh skill buttons (cooldowns)
        RefreshSkillButtons();
    }

    public void RefreshSkillButtons()
    {
        if (playerStats == null) return;
        
        List<SkillModel> availableSkills = playerStats.AvailableSkills.FindAll(s => !s.isPassive);
        
        for (int i = 0; i < skillContainer.childCount && i < availableSkills.Count; i++)
        {
            Transform buttonObj = skillContainer.GetChild(i);
            SkillModel skill = availableSkills[i];

            TextMeshProUGUI cooldownText = buttonObj.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
            Button btn = buttonObj.GetComponent<Button>();

            FighterData playerFighter = TurnManager.Instance.GetCurrentFighterModel();
            
            if (cooldownText != null && playerFighter != null)
            {
                if (playerFighter.IsSkillAvailable(skill))
                    cooldownText.text = "";
                else
                    cooldownText.text = $"CD: {playerFighter.GetSkillCooldown(skill.id)}";
            }

            if (btn != null && playerFighter != null)
            {
                btn.interactable = playerFighter.IsSkillAvailable(skill);
            }
        }
        scrollRect.horizontalNormalizedPosition = 0f;
    }

}
