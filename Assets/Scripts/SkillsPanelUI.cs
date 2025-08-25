using System.Collections.Generic;
using UnityEngine;

public class SkillsPanelUI : MonoBehaviour
{
    [SerializeField]
    private Transform skillsContainer;
    [SerializeField]
    private GameObject skillRowPrefab;
    [SerializeField]
    private CharacterStats player;


    public void RefreshUI()
    {
        if (!Application.isPlaying) return;

        foreach (Transform child in skillsContainer)
            Destroy(child.gameObject);
        if (skillRowPrefab == null || skillsContainer == null || SkillManager.Instance == null) return;

        foreach (var skill in player.AvailableSkills.FindAll(s => s.isLearned))
        {
            GameObject obj = Instantiate(skillRowPrefab, skillsContainer);
            var row = obj.GetComponent<SkillRowUI>();
            row.Setup(skill);
        }
    }
}
