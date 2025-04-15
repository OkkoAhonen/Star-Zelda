using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    public string questId;
    public bool autoStart = false;

    private bool questStarted = false;

    private void Start()
    {
        if (autoStart) TryStartQuest();
    }

    public void TryStartQuest()
    {
        if (questStarted) return;

        Quest quest = QuestManager.instance.GetQuestById(questId);
        if (quest != null && quest.state == Quest.QuestState.INACTIVE)
        {
            QuestManager.instance.StartQuest(quest);
            questStarted = true;
        }
    }

    public void Interact()
    {
        if (!questStarted) TryStartQuest();
        else
        {
            Quest quest = QuestManager.instance.GetQuestById(questId);
            if (quest != null && quest.state == Quest.QuestState.CAN_FINISH)
            {
                QuestManager.instance.CompleteQuest(quest);
            }
        }
    }
}
