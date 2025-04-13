using UnityEngine;
using System;

public class QuestStep : MonoBehaviour
{
    public string stepId; // optional — could match to inventory item, enemy, etc.

    public enum QuestStepState { INCOMPLETE, COMPLETE }

    [HideInInspector]
    public int stepIndex;

    public Action<QuestStepState> onStepComplete;

    public void CompleteStep()
    {
        onStepComplete?.Invoke(QuestStepState.COMPLETE);
    }

    // Can call this from triggers, events, etc.
    public void TryAutoComplete(string conditionId)
    {
        if (!string.IsNullOrEmpty(stepId) && conditionId == stepId)
        {
            CompleteStep();
        }
    }
}
