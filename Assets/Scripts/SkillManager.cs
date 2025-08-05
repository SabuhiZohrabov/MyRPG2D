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

    [Header("Skill Data")]
    public List<SkillModel> availableSkills = new List<SkillModel>();

    private SkillModel selectedSkill;
    private bool isTargetSelectionActive = false;

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
        List<SkillModel> filteredSkills = availableSkills.FindAll(s => s.isLearned && !s.isPassive);
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

        if (!skill.IsAvailable())
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
                if (targetModel.isAlive)
                {
                    targetModel.TakeDamage(selectedSkill.power);
                    CombatLog.Instance.AddLog($"<color=yellow>{selectedSkill.name}</color> hit <color=red>{targetModel.displayName}</color> for <b>{selectedSkill.power}</b> damage.");
                    if (!targetModel.isAlive)
                    {
                        CombatLog.Instance.AddLog($"<color=red>{targetModel.displayName}</color> was <b>defeated</b>!");
                    }
                }
                break;

            case SkillEffectType.Heal:
                targetModel.Heal(selectedSkill.power);
                CombatLog.Instance.AddLog($"<color=green>{selectedSkill.name}</color> healed <color=#00FFFF>{targetModel.displayName}</color> for <b>{selectedSkill.power}</b> HP.");
                break;

            case SkillEffectType.AreaDamage:
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
        selectedSkill.currentCooldown = selectedSkill.cooldown;
        RefreshSkillButtons();
    }

    public void ReduceCooldowns()
    {
        foreach (SkillModel skill in availableSkills)
        {
            if (skill.currentCooldown > 0)
                skill.currentCooldown--;
        }

        RefreshSkillButtons();  
    }

    public void RefreshSkillButtons()
    {
        for (int i = 0; i < skillContainer.childCount; i++)
        {
            Transform buttonObj = skillContainer.GetChild(i);
            SkillModel skill = availableSkills[i];

            TextMeshProUGUI cooldownText = buttonObj.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
            Button btn = buttonObj.GetComponent<Button>();

            if (cooldownText != null)
            {
                if (skill.IsAvailable())
                    cooldownText.text = "";
                else
                    cooldownText.text = $"CD: {skill.currentCooldown}";
            }

            if (btn != null)
            {
                btn.interactable = skill.IsAvailable();
            }
        }
        scrollRect.horizontalNormalizedPosition = 0f;
    }

}
