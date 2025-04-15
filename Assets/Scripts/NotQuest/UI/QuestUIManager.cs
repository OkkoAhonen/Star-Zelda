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

        var allQuests = QuestManager.instance.GetActiveQuests().Values.ToList();
        var completedQuests = QuestManager.instance.GetAllQuests().Where(q => q.state == Quest.QuestState.FINISHED).ToList();

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
}
