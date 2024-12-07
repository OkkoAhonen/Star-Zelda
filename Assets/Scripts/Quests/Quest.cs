using UnityEngine;

[System.Serializable]
public class Quest
{
    public string QuestID; // Teht�v�n yksil�llinen tunniste
    public string QuestName; // Teht�v�n nimi
    public string QuestDescription; // Teht�v�n kuvaus
    public bool IsCompleted; // Onko teht�v� valmis

    public QuestObjective[] Objectives; // Teht�v�n tavoitteet

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
