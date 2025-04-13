using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public List<Quest> allQuests = new List<Quest>();
    private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public void StartQuest(Quest quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.id))
            return;

        quest.state = Quest.QuestState.IN_PROGRESS;
        quest.InitializeRuntimeState();
        activeQuests.Add(quest.id, quest);

        QuestStepManager.instance.LoadStep(quest);

        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    public void CompleteQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.id)) return;

        quest.state = Quest.QuestState.FINISHED;
        // Rewards can be granted here: XP, gold, items, etc.
        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    public void TriggerStepCondition(string conditionId)
    {
        QuestStepManager.instance.TriggerStepCondition(conditionId);
    }

    public bool IsQuestActive(string questId)
    {
        return activeQuests.ContainsKey(questId);
    }

    public Quest GetQuestById(string questId)
    {
        return allQuests.Find(q => q.id == questId);
    }

    public Dictionary<string, Quest> GetActiveQuests()
    {
        return activeQuests;
    }
}
