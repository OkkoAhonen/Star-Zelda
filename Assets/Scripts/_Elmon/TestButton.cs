using UnityEngine;
using UnityEngine.UI;

public class GainExperienceButton : MonoBehaviour
{
    [Header("Button Settings")]
    public float experienceAmount = 10f;
    public Quest[] questsToComplete;

    private void Start()
    {
    }

    public void GainExperience()
    {
        GameEventsManager.instance.playerEvents.GainExperience(experienceAmount);
    }

    public void QuestDone()
    {
        if (questsToComplete == null)
        {
            var activeQuests = QuestManager.instance.GetActiveQuests().Values;
            foreach (Quest quest in QuestManager.instance.allQuests)
            {
                QuestManager.instance.StartQuest(quest);
            }
            foreach (Quest quest in activeQuests)
            {
                QuestManager.instance.CompleteQuest(quest);
            }
        }
        else
        {
            var activeQuests = QuestManager.instance.GetActiveQuests().Values;
            foreach (Quest quest in questsToComplete)
            {
                QuestManager.instance.StartQuest(quest);
            }
            foreach (Quest quest in questsToComplete)
            {
                QuestManager.instance.CompleteQuest(quest);
            }
        }
    }
}
