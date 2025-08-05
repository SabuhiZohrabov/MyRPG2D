using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private HashSet<string> activeQuests = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AcceptQuest(string questId)
    {
        if (!activeQuests.Contains(questId))
        {
            activeQuests.Add(questId);
            Debug.Log($"Quest accepted: {questId}");
        }
    }

    public bool HasQuest(string questId)
    {
        return activeQuests.Contains(questId);
    }
}
