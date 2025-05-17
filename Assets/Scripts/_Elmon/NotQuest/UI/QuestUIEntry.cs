using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class QuestUIEntry : MonoBehaviour
{
    public static bool closeOtherEntries = true;
    private static List<QuestUIEntry> allEntries = new();
    
    public float howGrayedOut = 0.5f;
    public CanvasGroup canvasGroup;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI questGiverText;

    public GameObject rewardSection;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI experienceText;
    public GameObject goldIconObject;
    public GameObject experienceIconObject;
    public Transform itemIconContainer;
    public GameObject itemIconPrefab;
    public Transform stepListContainer;
    public GameObject stepEntryPrefab;
    public Button trackQuestButton;
    public Image trackQuestButtonImage; // Assign the button's image component in the inspector
    public TextMeshProUGUI questDifficulty;

    private Color trackedColor = Color.green;
    private Color untrackedColor = Color.red;

    private Quest currentQuest;
    public bool IsForQuest(Quest q) => currentQuest == q;

    private void Awake()
    {
        allEntries.Add(this);
        GameEventsManager.instance.questEvents.onQuestStepStateChange += StepChangeData;
    }

    private void StepChangeData(string id, int index, Quest.QuestStepState state)
    {
        if (currentQuest == null || currentQuest.id != id)
            return;

        SetStepData();
    }

    private void OnDestroy()
    {
        allEntries.Remove(this);
        GameEventsManager.instance.questEvents.onQuestStepStateChange -= StepChangeData;
    }

    public void OnClicked()
    {
        bool isCurrentlyExpanded = IsExpanded();

        if (closeOtherEntries)
        {
            foreach (var entry in allEntries)
                entry.SetExpanded(false);
        }

        SetExpanded(!isCurrentlyExpanded);
    }

    private bool IsExpanded()
    {
        return transform.childCount > 1 && transform.GetChild(1).gameObject.activeSelf;
    }

    private void SetExpanded(bool expanded)
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(expanded);
        }
    }

    private void ToggleTrackQuest(Quest quest)
    {
        if (QuestTrackerUI.instance.IsTracked(quest))
            QuestTrackerUI.instance.UntrackQuest(quest);
        else
            QuestTrackerUI.instance.TrackQuest(quest);

        UpdateTrackButtonVisual(quest);
    }

    private void UpdateTrackButtonVisual(Quest quest)
    {
        bool isTracked = QuestTrackerUI.instance.IsTracked(quest);
        trackQuestButtonImage.color = isTracked ? trackedColor : untrackedColor;
    }

    public void SetData(Quest quest)
    {
        currentQuest = quest;

        if (QuestTrackerUI.instance.GetTrackedQuests().Count == 0)
        {
            QuestTrackerUI.instance.TrackQuest(quest);
        }

        questDifficulty.text = new string('I', quest.questDifficulty); // ★ *

        trackQuestButton.onClick.RemoveAllListeners();
        trackQuestButton.onClick.AddListener(() =>
        {
            ToggleTrackQuest(currentQuest);
        });
        UpdateTrackButtonVisual(currentQuest);

        titleText.text = quest.displayName;
        stateText.text = quest.state.ToString();

        bool isCompleted = quest.state == Quest.QuestState.FINISHED;
        descriptionText.text = quest.description;
        questGiverText.text = "From: " + quest.questGiverName;
        
        canvasGroup.alpha = isCompleted ? howGrayedOut : 1f;

        // Rewards
        bool hasRewards = false;

        if (quest.rewards.gold > 0)
        {
            goldText.gameObject.SetActive(true);
            goldIconObject.SetActive(true);
            goldText.text = quest.rewards.gold.ToString() + "G";
            hasRewards = true;
        }
        else
        {
            goldText.gameObject.SetActive(false);
            goldIconObject.SetActive(false);
        }

        // EXPERIENCE
        if (quest.rewards.experience > 0)
        {
            experienceText.gameObject.SetActive(true);
            experienceIconObject.SetActive(true);
            experienceText.text = quest.rewards.experience.ToString() + "XP";
            hasRewards = true;
        }
        else
        {
            experienceText.gameObject.SetActive(false);
            experienceIconObject.SetActive(false);
        }

        // Item rewards
        foreach (Transform child in itemIconContainer) Destroy(child.gameObject);
        if (quest.rewards.itemIcons != null && quest.rewards.itemIcons.Count > 0)
        {
            hasRewards = true;
            foreach (var icon in quest.rewards.itemIcons)
            {
                GameObject iconObj = Instantiate(itemIconPrefab, itemIconContainer);
                iconObj.GetComponent<Image>().sprite = icon;
            }
        }

        rewardSection.SetActive(hasRewards);

        SetStepData();

        SetExpanded(false); // Collapsed by default (might be a little broken if here)
    }

    public void SetStepData()
    {

        // Clear existing step entries
        foreach (Transform child in stepListContainer)
        {

            Debug.Log("Debugataan" + child.name);
            Destroy(child.gameObject);
            

        }

        // Display steps
        for (int i = 0; i < currentQuest.steps.Count; i++)
        {
            var step = currentQuest.steps[i];
            var state = currentQuest.stepStates[i];
            var progress = currentQuest.stepProgress[i];

            string progressText = step.stepType switch
            {
                QuestStepData.StepType.Kill => $"{step.stepType}: {progress} / {step.requiredAmount}",
                QuestStepData.StepType.Gather => $"{step.stepType}: {progress} / {step.requiredAmount}",
                QuestStepData.StepType.Visit => state == Quest.QuestStepState.COMPLETE ? $"{step.stepType}: Complete" : $"{step.stepType}: Incomplete",
                QuestStepData.StepType.Give => state == Quest.QuestStepState.COMPLETE ? $"{step.stepType}: Complete" : $"{step.stepType}: Incomplete",
                _ => "Unknown step"
            };

            progressText += " Diff " + new string('I', currentQuest.questDifficulty);

            var entry = Instantiate(stepEntryPrefab, stepListContainer);
            entry.GetComponent<TextMeshProUGUI>().text = progressText;
        }
    }
}
