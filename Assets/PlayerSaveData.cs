using UnityEngine; // Tarvitaan Vector2:lle

[System.Serializable] // Tämä on tärkeä, jotta JsonUtility voi käsitellä tätä luokkaa
public class PlayerSaveData
{
    public int currentHealth;
    public int Level;
    public int NumberofItems;
    public int Gold;
    public string playerName; // Muutin "name" -> "playerName" koska "name" on varattu Unityssä
    public string playerLastname; // Muutin "lastname" -> "playerLastname"
    public bool haveYouDiedYet; // Korjasin kirjoitusvirheen
    public Vector2 playerLocation;
}