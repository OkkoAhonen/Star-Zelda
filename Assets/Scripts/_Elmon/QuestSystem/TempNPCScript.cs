using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempNPCScript : MonoBehaviour
{
    public bool canStartQuest;

    private void Awake()
    {
        GameEventsManager.instance.inputEvents.OnSubmitPressed += StartQuestTemp;
    }

    private void StartQuestTemp(InputEventContext context)
    {
        if (!canStartQuest)
            return;
        QuestPoint questPoint = GetComponent<QuestPoint>();
        questPoint.Interact();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canStartQuest)
            if (collision.transform.name == "Player")
                canStartQuest = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (canStartQuest)
            if (collision.transform.name == "Player")
                canStartQuest = false;
    }
}
