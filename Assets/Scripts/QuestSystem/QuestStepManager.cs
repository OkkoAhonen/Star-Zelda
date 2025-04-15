using UnityEngine;

public class QuestStepManager : MonoBehaviour
{
    public static QuestStepManager instance;
    public Transform stepParent;

    private Quest currentQuest;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    public void LoadSteps(Quest quest)
    {
        ClearCurrentSteps();
        currentQuest = quest;

        for (int i = 0; i < quest.steps.Count; i++)
        {
            if (quest.stepStates[i] == QuestStep.QuestStepState.INCOMPLETE)
            {
                QuestStep step = Instantiate(quest.steps[i], stepParent);
                step.stepIndex = i;
                step.onStepComplete += HandleStepComplete;
            }
        }
    }

    private void HandleStepComplete(QuestStep.QuestStepState state)
    {
        var stepObj = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.GetComponent<QuestStep>();
        if (stepObj == null) return;

        currentQuest.MarkStepComplete(stepObj.stepIndex);
        Destroy(stepObj.gameObject);
    }

    private void ClearCurrentSteps()
    {
        foreach (Transform child in stepParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void TriggerStepCondition(string conditionId)
    {
        foreach (Transform stepTransform in stepParent)
        {
            var step = stepTransform.GetComponent<QuestStep>();
            if (step != null)
            {
                step.TryAutoComplete(conditionId);
            }
        }
    }
}
