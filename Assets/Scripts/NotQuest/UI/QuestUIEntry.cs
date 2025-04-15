using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class QuestUIEntry : MonoBehaviour
{
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

    public void SetData(Quest quest)
    {
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
    }
}
