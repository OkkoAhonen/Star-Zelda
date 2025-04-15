using UnityEngine;
using System;

public class QuestStep : MonoBehaviour
{
    public enum QuestStepState { INCOMPLETE, COMPLETE }
    public enum StepType { FETCH, KILL, TALK }

    public string stepId;
    public StepType stepType;

    [HideInInspector] public int stepIndex;
    public Action<QuestStepState> onStepComplete;

    public void CompleteStep() => onStepComplete?.Invoke(QuestStepState.COMPLETE);

    public void TryAutoComplete(string conditionId)
    {
        if (!string.IsNullOrEmpty(stepId) && conditionId == stepId)
        {
            CompleteStep();
        }
    }
}
