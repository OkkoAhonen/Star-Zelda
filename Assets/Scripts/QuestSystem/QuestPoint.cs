using UnityEngine;
using UnityEngine.UI;

public class QuestPoint : MonoBehaviour
{
    public GameObject questIcon;
    public Sprite iconAvailable;
    public Sprite iconComplete;
    public Sprite iconInProgress;

    public string questId;
    public bool autoStart = false;

    private bool questStarted = false;
    private Quest quest;

    private void Start()
    {
        quest = QuestManager.instance.GetQuestById(questId);

        if (autoStart && quest.state == Quest.QuestState.CAN_START)
            TryStartQuest();

        GameEventsManager.instance.questEvents.onQuestStateChange += UpdateIconIndirectly;
        UpdateIcon();
    }

    private void UpdateIconIndirectly(Quest quest)
    {
        UpdateIcon();
    }

    public void TryStartQuest()
    {
        if (questStarted)
        {
            Debug.LogWarning("Quest has already started");
            return;
        }
        if (quest == null)
        {
            Debug.LogWarning("[QuestPoint] Quest is null!");
            return;
        }

        if (quest.state == Quest.QuestState.CAN_START) // quest.state == Quest.QuestState.INACTIVE || 
        {
            QuestManager.instance.StartQuest(quest);
            questStarted = true;
            Debug.Log($"[QuestPoint] Quest: {quest.displayName}, id: {questId} started!");
            return;
        }
        
        Debug.Log($"Quest: {quest.displayName}, id: {questId}, state: {quest.state}  couldn't start quest");
    }

    public void Interact()
    {
        if (!questStarted) TryStartQuest();
        else if (quest.state == Quest.QuestState.CAN_FINISH)
        {
            QuestManager.instance.CompleteQuest(quest);
            Debug.Log($"[QuestPoint] Quest {quest.displayName} completed!");
        }
    }

    void UpdateIcon()
    {
        SpriteRenderer questIconSprite = questIcon.GetComponent<SpriteRenderer>();
        if (quest == null)
        {
            questIcon.SetActive(false);
            return;
        }

        questIcon.SetActive(true);

        switch (quest.state)
        {
            case Quest.QuestState.CAN_START:
                questIconSprite.sprite = iconAvailable; break;
            case Quest.QuestState.IN_PROGRESS:
                questIconSprite.sprite = iconInProgress; break;
            case Quest.QuestState.CAN_FINISH:
                questIconSprite.sprite = iconComplete; break;
            default:
                questIcon.SetActive(false); break;
        }
    }
}
