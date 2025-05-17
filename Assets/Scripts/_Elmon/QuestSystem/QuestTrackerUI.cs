using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI instance;
    private List<Quest> trackedQuests = new();    
    public TextMeshProUGUI questTitleText;
    public Transform stepsContainer;
    public GameObject stepTextPrefab;

    private Quest currentQuest;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        GameEventsManager.instance.questEvents.onQuestStepStateChange += UpdateSteps;
        GameEventsManager.instance.questEvents.onQuestStateChange += OnQuestStateChanged;
    }

    public void TrackQuest(Quest quest)
    {
        if (!trackedQuests.Contains(quest))
        {
            trackedQuests.Add(quest);
            currentQuest = quest;
            RefreshUI();
            Debug.Log("Now tracking: " + quest.displayName);
        }
    }

    public bool IsTracked(Quest quest) => trackedQuests.Contains(quest);

    public void UntrackQuest(Quest quest)
    {
        if (trackedQuests.Contains(quest))
        {
            trackedQuests.Remove(quest);
            Debug.Log("Untracked: " + quest.displayName);

            // If the current quest was untracked, clear or update UI
            if (currentQuest == quest)
            {
                currentQuest = trackedQuests.Count > 0 ? trackedQuests[0] : null;
                if (currentQuest != null)
                    RefreshUI();
                else
                    ClearUI();
            }
        }
    }

    public List<Quest> GetTrackedQuests() => trackedQuests;

    private void OnDestroy()
    {
        GameEventsManager.instance.questEvents.onQuestStepStateChange -= UpdateSteps;
        GameEventsManager.instance.questEvents.onQuestStateChange -= OnQuestStateChanged;
    }

    private void OnQuestStateChanged(Quest quest)
    {
        if (quest.state == Quest.QuestState.IN_PROGRESS)
        {
            currentQuest = quest;
            RefreshUI();
        }

        // If finished, optionally clear it:
        if (quest.state == Quest.QuestState.FINISHED && currentQuest?.id == quest.id)
        {
            currentQuest = null;
            ClearUI();
        }
    }

    private void UpdateSteps(string questId, int stepIndex, Quest.QuestStepState state)
    {
        if (currentQuest != null && currentQuest.id == questId)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (currentQuest == null) return;

        questTitleText.text = $"{currentQuest.displayName}";

        foreach (Transform child in stepsContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < currentQuest.steps.Count; i++)
        {
            var step = currentQuest.steps[i];
            var state = currentQuest.stepStates[i];
            
            string status = state == Quest.QuestStepState.COMPLETE ? "[Done]" : "[In progress]";
            string label = $"{step.stepType}: {step.stepName} {i + 1} {status}";

            var stepObj = Instantiate(stepTextPrefab, stepsContainer);
            stepObj.GetComponentInChildren<TextMeshProUGUI>().text = label;
        }
    }

    private void ClearUI()
    {
        questTitleText.text = "No active quest";
        foreach (Transform child in stepsContainer)
            Destroy(child.gameObject);
    }
}
