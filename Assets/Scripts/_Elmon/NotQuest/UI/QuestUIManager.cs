using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class QuestUIManager : MonoBehaviour
{
    public GameObject questEntryPrefab; // Reference to the prefab
    public Transform contentParent;     // ScrollView content parent

    private List<QuestUIEntry> activeEntries = new List<QuestUIEntry>();
    private static int completedQuestCounter = 0;

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += OnQuestStateChanged;
        RefreshUI();
    }

    private void OnQuestStateChanged(Quest quest)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Clear existing UI entries
        foreach (var entry in activeEntries)
        {
            Destroy(entry.gameObject);
        }
        activeEntries.Clear();

        var activeQuestsDict = QuestManager.instance.GetActiveQuests();
        var allQuestsList = QuestManager.instance.GetAllQuests();

        var allQuests = activeQuestsDict?.Values?.ToList() ?? new List<Quest>();
        var completedQuests = allQuestsList?.Where(q => q.state == Quest.QuestState.FINISHED).ToList() ?? new List<Quest>();

        // Set order for completed quests if not already set
        foreach (var quest in completedQuests)
        {
            if (!quest.wasCompleted)
            {
                quest.wasCompleted = true;
                quest.completionOrder = completedQuestCounter++;
            }
        }

        // Sort
        var sortedActive = allQuests
            .Where(q => q.state != Quest.QuestState.FINISHED)
            .OrderBy(q => q.questImportance)
            .ToList();

        var sortedCompleted = completedQuests
            .OrderBy(q => q.completionOrder)
            .ToList();

        var sortedAll = new List<Quest>();
        sortedAll.AddRange(sortedActive);
        sortedAll.AddRange(sortedCompleted);

        // Populate UI
        foreach (var quest in sortedAll)
        {
            GameObject entryGO = Instantiate(questEntryPrefab, contentParent);
            QuestUIEntry entry = entryGO.GetComponent<QuestUIEntry>();
            entry.SetData(quest);
            activeEntries.Add(entry);
        }
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= OnQuestStateChanged;
    }
}
