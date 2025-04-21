using System;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance;

    public InputEvents inputEvents;
    public PlayerEvents playerEvents;
    public MiscEvents miscEvents;
    public QuestEvents questEvents;
    public DialogueEvents dialogueEvents;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            //Debug.LogError("Found more than one Game Events Manager in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;

        // initialize all events
        inputEvents = new InputEvents();
        playerEvents = new PlayerEvents();
        miscEvents = new MiscEvents();
        questEvents = new QuestEvents();
        dialogueEvents = new DialogueEvents();
    }
}