using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventVisual : MonoBehaviour
{
    [System.Serializable]
    public class DisplayEntry
    {
        public bool isQuest;
        public string message;   // for quests
        public int startLevel;   // for levels
        public int endLevel;     // for levels

        public override string ToString()
        {
            return isQuest
                ? $"Quest: {message}"
                : $"Level {startLevel} -> Level {endLevel}";
        }
    }

    [Header("UI Elements")]
    public GameObject everything;
    public GameObject questTitle;
    public GameObject levelUpTitle;
    public TMP_Text whatChanged;
    public TMP_Text whatChangedShadow;

    [Header("Movement Settings")]
    public Vector3 offScreenOffset = new Vector3(0f, 500f, 0f);
    public float moveSpeed = 1000f;
    public float snapTime = 1f;

    [Header("Spin Settings")]
    public float spinDegrees = 360f;
    public float spinSpeed = 360f;

    [Header("Runtime Queue (read-only)")]
    [SerializeField]
    private List<DisplayEntry> displayQueue = new List<DisplayEntry>();

    private bool isProcessing = false;
    private DisplayEntry currentEntry = null;
    private bool inSlidePhase = false;

    private Vector3 endPosition;
    private Vector3 startPosition;

    private void Awake()
    {
        everything.SetActive(false);
        questTitle.SetActive(false);
        levelUpTitle.SetActive(false);
        whatChanged.gameObject.SetActive(false);
        whatChangedShadow.gameObject.SetActive(false);
    }

    private void Start()
    {
        endPosition = everything.transform.position;
        startPosition = endPosition + offScreenOffset;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.playerEvents.onPlayerLevelChangeTo += HandleLevelChange;
        GameEventsManager.instance.questEvents.onFinishQuest += HandleQuestDone;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.playerEvents.onPlayerLevelChangeTo -= HandleLevelChange;
        GameEventsManager.instance.questEvents.onFinishQuest -= HandleQuestDone;
    }

    private void HandleQuestDone(string questName)
    {
        var entry = new DisplayEntry
        {
            isQuest = true,
            message = questName
        };
        displayQueue.Add(entry);
        TryStartProcessing();
    }

    private void HandleLevelChange(int newLevel)
    {
        // 1) If sliding in a level, update currentEntry
        if (isProcessing && currentEntry != null && !currentEntry.isQuest && inSlidePhase)
        {
            currentEntry.endLevel = newLevel;
            currentEntry.message = $"Level {currentEntry.startLevel} -> Level {currentEntry.endLevel}";
            return;
        }

        // 2) Else if there's already a level entry pending in the queue, update it
        var idx = displayQueue.FindIndex(e => !e.isQuest);
        if (idx >= 0)
        {
            var pending = displayQueue[idx];
            pending.endLevel = newLevel;
            pending.message = $"Level {pending.startLevel} -> Level {pending.endLevel}";
            return;
        }

        // 3) Otherwise, enqueue a new level entry
        int fromLevel = Mathf.Max(1, newLevel - 1);
        var entry = new DisplayEntry
        {
            isQuest = false,
            startLevel = fromLevel,
            endLevel = newLevel,
            message = $"Level {fromLevel} -> Level {newLevel}"
        };
        displayQueue.Add(entry);
        TryStartProcessing();
    }

    private void TryStartProcessing()
    {
        if (!isProcessing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isProcessing = true;

        while (displayQueue.Count > 0)
        {
            currentEntry = displayQueue[0];
            displayQueue.RemoveAt(0);
            yield return StartCoroutine(ShowDisplay(currentEntry));
            currentEntry = null;
        }

        isProcessing = false;
    }

    private IEnumerator ShowDisplay(DisplayEntry entry)
    {
        // reset
        everything.transform.rotation = Quaternion.identity;
        everything.transform.position = startPosition;
        everything.SetActive(true);

        // enable correct title immediately
        questTitle.SetActive(entry.isQuest);
        levelUpTitle.SetActive(!entry.isQuest);

        // hide text until after slide
        whatChanged.gameObject.SetActive(false);
        whatChangedShadow.gameObject.SetActive(false);

        // slide in
        inSlidePhase = true;
        while (Vector3.Distance(everything.transform.position, endPosition) > 0.1f)
        {
            everything.transform.position = Vector3.MoveTowards(
                everything.transform.position,
                endPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        everything.transform.position = endPosition;
        inSlidePhase = false;

        // set and show text
        whatChanged.text = entry.message;
        whatChangedShadow.text = entry.message;
        whatChanged.gameObject.SetActive(true);
        whatChangedShadow.gameObject.SetActive(true);

        // wait before spin
        yield return new WaitForSeconds(snapTime);

        // spin
        float rotated = 0f;
        while (rotated < spinDegrees)
        {
            float step = spinSpeed * Time.deltaTime;
            everything.transform.Rotate(Vector3.forward, step);
            rotated += step;
            yield return null;
        }

        // cleanup
        everything.transform.rotation = Quaternion.identity;
        everything.SetActive(false);
        questTitle.SetActive(false);
        levelUpTitle.SetActive(false);
    }
}
