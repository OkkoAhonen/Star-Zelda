using UnityEngine;

public class QuestStepManager : MonoBehaviour
{
    public static QuestStepManager instance;
    public Transform stepParent;

    private Quest currentQuest;
    private int currentStepIndex;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void LoadStep(Quest quest)
    {
        ClearCurrentSteps();
        currentQuest = quest;

        for (int i = 0; i < quest.steps.Count; i++)
        {
            if (quest.stepStates[i] == QuestStep.QuestStepState.INCOMPLETE)
            {
                QuestStep stepInstance = Instantiate(quest.steps[i], stepParent);
                stepInstance.stepIndex = i;
                stepInstance.onStepComplete += HandleStepComplete;
            }
        }
    }

    private void HandleStepComplete(QuestStep.QuestStepState state)
    {
        var step = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.GetComponent<QuestStep>();
        if (step == null) return;

        currentQuest.MarkStepComplete(step.stepIndex);
        Destroy(step.gameObject);
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
        foreach (Transform child in stepParent)
        {
            QuestStep step = child.GetComponent<QuestStep>();
            if (step != null)
                step.TryAutoComplete(conditionId);
        }
    }
}
