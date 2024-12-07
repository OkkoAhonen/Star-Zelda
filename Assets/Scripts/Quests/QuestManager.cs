using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance; // Singleton, helppo k‰ytt‰‰ miss‰ tahansa

    private List<Quest> activeQuests = new List<Quest>(); // Lista aktiivisista teht‰vist‰
    private List<Quest> completedQuests = new List<Quest>(); // Lista suoritetuista teht‰vist‰

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Lis‰‰ uusi teht‰v‰ aktiivisiin teht‰viin
    public void AddQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            Debug.Log($"Teht‰v‰ '{quest.QuestName}' lis‰tty.");
        }
        else
        {
            Debug.LogWarning($"Teht‰v‰ '{quest.QuestName}' on jo lis‰tty.");
        }
    }

    // P‰ivitt‰‰ teht‰v‰n tilaa
    public void UpdateQuestProgress(string questID, int objectiveIndex)
    {
        Quest quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.Objectives.Length)
        {
            quest.Objectives[objectiveIndex].IsCompleted = true;
            Debug.Log($"Tavoite '{quest.Objectives[objectiveIndex].ObjectiveDescription}' suoritettu.");

            // Tarkista, onko teht‰v‰ valmis
            if (quest.AreAllObjectivesComplete())
            {
                CompleteQuest(quest);
            }
        }
        else
        {
            Debug.LogWarning("Tavoitteen p‰ivitys ep‰onnistui. Tarkista QuestID tai ObjectiveIndex.");
        }
    }

    // Merkitsee teht‰v‰n suoritetuksi
    private void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        quest.IsCompleted = true;
        Debug.Log($"Teht‰v‰ '{quest.QuestName}' suoritettu!");
    }

    // Palauttaa kaikki aktiiviset teht‰v‰t
    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }

    // Palauttaa kaikki suoritetut teht‰v‰t
    public List<Quest> GetCompletedQuests()
    {
        return completedQuests;
    }
}
