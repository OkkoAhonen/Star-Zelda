using UnityEngine;

[System.Serializable]
public class Quest
{
    public string QuestID; // Tehtävän yksilöllinen tunniste
    public string QuestName; // Tehtävän nimi
    public string QuestDescription; // Tehtävän kuvaus
    public bool IsCompleted; // Onko tehtävä valmis

    public QuestObjective[] Objectives; // Tehtävän tavoitteet

    // Tarkistaa, onko kaikki tavoitteet suoritettu
    public bool AreAllObjectivesComplete()
    {
        foreach (var objective in Objectives)
        {
            if (!objective.IsCompleted)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class QuestObjective
{
    public string ObjectiveDescription; // Tavoitteen kuvaus
    public bool IsCompleted; // Onko tavoite suoritettu
}
