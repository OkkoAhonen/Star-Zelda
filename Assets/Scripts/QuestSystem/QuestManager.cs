using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public List<Quest> allQuests = new();
    private Dictionary<string, Quest> activeQuests = new();

    private void Awake()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
    }

    public void StartQuest(Quest quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.id)) return;

        quest.state = Quest.QuestState.IN_PROGRESS;
        quest.InitializeRuntimeState();
        activeQuests.Add(quest.id, quest);

        GameEventsManager.instance.questEvents.StartQuest(quest.id);
        GameEventsManager.instance.questEvents.QuestStateChange(quest);

        QuestStepManager.instance.LoadSteps(quest);
    }
    public List<Quest> GetAllQuests()
    {
        return allQuests;
    }

    public void CompleteQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.id)) return;

        quest.state = Quest.QuestState.FINISHED;

        GameEventsManager.instance.questEvents.FinishQuest(quest.id);
        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    public void AdvanceQuest(string questId)
    {
        GameEventsManager.instance.questEvents.AdvanceQuest(questId);
    }

    public void TriggerStepCondition(string conditionId)
    {
        QuestStepManager.instance.TriggerStepCondition(conditionId);
    }

    public bool IsQuestActive(string questId) => activeQuests.ContainsKey(questId);

    public Quest GetQuestById(string questId) =>
        allQuests.Find(q => q.id == questId);

    public Dictionary<string, Quest> GetActiveQuests() => activeQuests;
}
