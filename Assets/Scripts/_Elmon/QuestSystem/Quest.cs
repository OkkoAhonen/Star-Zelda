using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public int gold;
    public int experience;
    public List<Sprite> itemIcons;
    public Item[] getQuestItem;
}

[System.Serializable]
public class QuestStepData
{
    public enum StepType { Kill, Gather, Visit, Give }
    public StepType stepType;

    public string stepName;

    public string targetId;     // Could be enemy ID, item ID, or location name
    public int requiredAmount = 1; // For kills/gathers
    public int stepDifficulty;
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    [HideInInspector] public bool wasCompleted = false; // Tracks completion order
    [HideInInspector] public int completionOrder = -1; // Index of when it was completed

    public string questGiverName;
    public int questImportance; // Used for sorting active quests
    public int questDifficulty;

    [HideInInspector] public List<QuestStepState> stepStates = new();
    public string description;
    public string id;
    public string displayName;

    [Header("Rewards")]
    public QuestReward rewards;

    public enum QuestState { INACTIVE, CAN_START, IN_PROGRESS, CAN_FINISH, FINISHED }
    public QuestState state = QuestState.CAN_START;

    [Header("Steps")]
    public List<QuestStepData> steps = new();
    [HideInInspector] public List<int> stepProgress = new(); // runtime progress tracking
    [HideInInspector] public enum QuestStepState { INCOMPLETE, COMPLETE }

    public void InitializeRuntimeState()
    {
        stepStates.Clear();
        stepProgress.Clear();

        for (int i = 0; i < steps.Count; i++)
        {
            stepStates.Add(QuestStepState.INCOMPLETE);
            stepProgress.Add(0);
        }
    }

    public void MarkStepComplete(int index)
    {
        if (index < 0 || index >= stepStates.Count) return;

        stepStates[index] = QuestStepState.COMPLETE;

        GameEventsManager.instance.questEvents.QuestStepStateChange(id, index, QuestStepState.COMPLETE);

        if (AllStepsComplete())
        {
            state = QuestState.CAN_FINISH;
            GameEventsManager.instance.questEvents.QuestStateChange(this);
        }
    }

    public bool AllStepsComplete()
    {
        foreach (var state in stepStates)
        {
            if (state != QuestStepState.COMPLETE) return false;
        }
        return true;
    }

    public void ResetSteps() => InitializeRuntimeState();

    [System.Serializable]
    public class QuestData
    {
        public string id;
        public QuestState state;
        public List<QuestStepState> stepStates;
    }

    public QuestData GetQuestData() => new QuestData
    {
        id = this.id,
        state = this.state,
        stepStates = new List<QuestStepState>(this.stepStates)
    };

    public void LoadQuestData(QuestData data)
    {
        this.state = data.state;
        this.stepStates = new List<QuestStepState>(data.stepStates);
    }
}
