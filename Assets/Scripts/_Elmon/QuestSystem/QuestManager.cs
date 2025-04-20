using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public List<Quest> allQuests = new();
    private Dictionary<string, Quest> activeQuests = new();

    private void Awake()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
#if UNITY_EDITOR
    ResetAllQuests();
#endif
    }

    public void StartQuest(Quest quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.id)) return;

        if (quest.state == Quest.QuestState.CAN_START || quest.state == Quest.QuestState.INACTIVE)
        {
            quest.state = Quest.QuestState.IN_PROGRESS;
            quest.InitializeRuntimeState();
            activeQuests.Add(quest.id, quest);

            GameEventsManager.instance.questEvents.StartQuest(quest.id);
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }
    }

    public void ResetAllQuests()
    {
#if UNITY_EDITOR
    foreach (Quest quest in allQuests)
    {
        quest.state = Quest.QuestState.CAN_START;
        quest.wasCompleted = false;
        quest.completionOrder = -1;
        quest.stepProgress.Clear();
        quest.stepStates.Clear();
    }
#endif
    }

    public List<Quest> GetAllQuests()
    {
        return allQuests;
    }

    public void CompleteQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.id)) return;

        quest.state = Quest.QuestState.FINISHED;

        if(quest.rewards.getQuestItem != null) {

            for(int i = 0; i < quest.rewards.getQuestItem.Length; i++) { 

            InventoryManager.Instance.AddItem(quest.rewards.getQuestItem[i]);
        }
        }
        GameEventsManager.instance.playerEvents.GainExperience(quest.rewards.experience);
        
        GameEventsManager.instance.questEvents.FinishQuest(quest.id);
        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    public void AdvanceQuest(string questId)
    {
        GameEventsManager.instance.questEvents.AdvanceQuest(questId);
    }

    public bool IsQuestActive(string questId) => activeQuests.ContainsKey(questId);

    public Quest GetQuestById(string questId) =>
        allQuests.Find(q => q.id == questId);

    public Dictionary<string, Quest> GetActiveQuests() => activeQuests;

    public void NotifyStepEvent(string eventType, string targetId)
    {
        foreach (Quest quest in activeQuests.Values)
        {
            for (int i = 0; i < quest.steps.Count; i++)
            {
                var step = quest.steps[i];
                if (quest.stepStates[i] == Quest.QuestStepState.COMPLETE) continue;

                bool matched = step.stepType.ToString().ToLower() == eventType.ToLower() && step.targetId == targetId;
                if (matched)
                {
                    quest.stepProgress[i]++;

                    if (quest.stepProgress[i] >= step.requiredAmount)
                    {
                        quest.MarkStepComplete(i);
                    }
                }
            }
        }
    }
}
