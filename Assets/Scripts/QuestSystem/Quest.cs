using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public int gold;
    public int experience;
    public List<Sprite> itemIcons;
}


[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    [HideInInspector] public bool wasCompleted = false; // Tracks completion order
    [HideInInspector] public int completionOrder = -1; // Index of when it was completed

    public string questGiverName;
    public int questImportance; // Used for sorting active quests


    public string description;
    public string id;
    public string displayName;

    [Header("Rewards")]
    public QuestReward rewards;

    public enum QuestState { INACTIVE, CAN_START, IN_PROGRESS, CAN_FINISH, FINISHED }
    public QuestState state = QuestState.INACTIVE;

    [Header("Steps")]
    public List<QuestStep> steps;

    [HideInInspector] public List<QuestStep.QuestStepState> stepStates = new();

    public void InitializeRuntimeState()
    {
        stepStates.Clear();
        for (int i = 0; i < steps.Count; i++)
        {
            stepStates.Add(QuestStep.QuestStepState.INCOMPLETE);
        }
    }

    public void MarkStepComplete(int index)
    {
        if (index < 0 || index >= stepStates.Count) return;

        stepStates[index] = QuestStep.QuestStepState.COMPLETE;

        GameEventsManager.instance.questEvents.QuestStepStateChange(id, index, QuestStep.QuestStepState.COMPLETE);

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
            if (state != QuestStep.QuestStepState.COMPLETE) return false;
        }
        return true;
    }

    public void ResetSteps() => InitializeRuntimeState();

    [System.Serializable]
    public class QuestData
    {
        public string id;
        public QuestState state;
        public List<QuestStep.QuestStepState> stepStates;
    }

    public QuestData GetQuestData() => new QuestData
    {
        id = this.id,
        state = this.state,
        stepStates = new List<QuestStep.QuestStepState>(this.stepStates)
    };

    public void LoadQuestData(QuestData data)
    {
        this.state = data.state;
        this.stepStates = new List<QuestStep.QuestStepState>(data.stepStates);
    }
}
