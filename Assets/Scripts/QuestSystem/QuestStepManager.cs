using System.Collections.Generic;
using UnityEngine;

public class QuestStepManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestStepData
    {
        public string questId;
        public QuestType questType;
        public string target; // e.g., "Coins", "Ancient Pillar", "EnemyGoblin"
        public int requiredAmount = 1;
        public int currentAmount = 0;
        public bool isCompleted = false;
    }

    public enum QuestType { Collect, Travel, Kill, Talk }

    private Dictionary<string, QuestStepData> questSteps = new Dictionary<string, QuestStepData>();

    private void Start()
    {
        GameEventsManager.instance.miscEvents.onCoinCollected += () => UpdateQuestStep("Coins");
        GameEventsManager.instance.playerEvents.onEnemyKilled += UpdateQuestStep;
        GameEventsManager.instance.dialogueEvents.onDialogueComplete += UpdateQuestStep;
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.miscEvents.onCoinCollected -= () => UpdateQuestStep("Coins");
        GameEventsManager.instance.playerEvents.onEnemyKilled -= UpdateQuestStep;
        GameEventsManager.instance.dialogueEvents.onDialogueComplete -= UpdateQuestStep;
    }

    public void AddQuestStep(string questId, QuestType type, string target, int requiredAmount)
    {
        string key = $"{questId}_{type}_{target}";
        questSteps[key] = new QuestStepData { questId = questId, questType = type, target = target, requiredAmount = requiredAmount };
    }

    private void UpdateQuestStep(string target)
    {
        List<string> keysToComplete = new List<string>();

        foreach (var kvp in questSteps)
        {
            var step = kvp.Value;
            if (step.target == target && !step.isCompleted)
            {
                step.currentAmount++;
                if (step.currentAmount >= step.requiredAmount)
                {
                    step.isCompleted = true;
                    keysToComplete.Add(kvp.Key);
                }
            }
        }

        // Complete quest steps after iteration to avoid modifying dictionary during iteration
        foreach (var key in keysToComplete)
        {
            GameEventsManager.instance.questEvents.AdvanceQuest(questSteps[key].questId);
            questSteps.Remove(key);
        }
    }
}
