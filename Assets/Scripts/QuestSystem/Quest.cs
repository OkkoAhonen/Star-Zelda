using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    public string id;
    public string displayName;
    public int goldReward;
    public int experienceReward;

    public enum QuestState { INACTIVE, CAN_START, IN_PROGRESS, CAN_FINISH, FINISHED }
    public QuestState state = QuestState.INACTIVE;

    [Header("Steps")]
    public List<QuestStep> steps;

    // runtime state — not stored in QuestStep!
    [HideInInspector]
    public List<QuestStep.QuestStepState> stepStates = new List<QuestStep.QuestStepState>();

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
        if (index >= 0 && index < stepStates.Count)
        {
            stepStates[index] = QuestStep.QuestStepState.COMPLETE;
        }

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
            if (state != QuestStep.QuestStepState.COMPLETE)
                return false;
        }
        return true;
    }

    public void ResetSteps()
    {
        InitializeRuntimeState();
    }

    [System.Serializable]
    public class QuestData
    {
        public string id;
        public QuestState state;
        public List<QuestStep.QuestStepState> stepStates;
    }

    public QuestData GetQuestData()
    {
        return new QuestData
        {
            id = this.id,
            state = this.state,
            stepStates = new List<QuestStep.QuestStepState>(this.stepStates)
        };
    }

    public void LoadQuestData(QuestData data)
    {
        this.state = data.state;
        this.stepStates = new List<QuestStep.QuestStepState>(data.stepStates);
    }
}
