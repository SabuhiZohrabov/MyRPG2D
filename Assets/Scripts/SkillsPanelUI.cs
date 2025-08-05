using System.Collections.Generic;
using UnityEngine;

public class SkillsPanelUI : MonoBehaviour
{
    public Transform skillsContainer;
    public GameObject skillRowPrefab;


    public void RefreshUI()
    {
        if (!Application.isPlaying) return;

        foreach (Transform child in skillsContainer)
            Destroy(child.gameObject);
        if (skillRowPrefab == null || skillsContainer == null || SkillManager.Instance == null) return;

        foreach (var skill in SkillManager.Instance.availableSkills.FindAll(s => s.isLearned))
        {
            GameObject obj = Instantiate(skillRowPrefab, skillsContainer);
            var row = obj.GetComponent<SkillRowUI>();
            row.Setup(skill);
        }
    }
}
