using UnityEngine; // Tarvitaan Vector2:lle

[System.Serializable] // T�m� on t�rke�, jotta JsonUtility voi k�sitell� t�t� luokkaa
public class PlayerSaveData
{
    public int currentHealth;
    public int Level;
    public int NumberofItems;
    public int Gold;
    public string playerName; // Muutin "name" -> "playerName" koska "name" on varattu Unityss�
    public string playerLastname; // Muutin "lastname" -> "playerLastname"
    public bool haveYouDiedYet; // Korjasin kirjoitusvirheen
    public Vector2 playerLocation;
}