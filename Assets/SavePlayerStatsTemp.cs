using UnityEngine;
// Poista using System.Collections.Generic; ja using Unity.VisualScripting.Antlr3.Runtime.Collections; jos et käytä niitä tässä skriptissä suoraan

public class SavePlayerStatsTemp : MonoBehaviour
{
    // Nämä ovat pelin AIKAISET arvot
    public int currentHealth = 100;
    public int Level = 1;
    public int NumberofItems = 0;
    public int Gold = 0;
    public string playerName = "Pelaaja";
    public string playerLastname = "Sukunimi";
    public bool haveYouDiedYet = false; // Korjattu kirjoitusvirhe

    // playerlocationia ei tarvitse olla tässä erikseen, koska se saadaan transform.positionista

    // Metodi, joka kerää nykyiset tiedot PlayerSaveData-olioon tallennusta varten
    public PlayerSaveData GetCurrentPlayerData()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.currentHealth = this.currentHealth;
        data.Level = this.Level;
        data.NumberofItems = this.NumberofItems;
        data.Gold = this.Gold;
        data.playerName = this.playerName;
        data.playerLastname = this.playerLastname;
        data.haveYouDiedYet = this.haveYouDiedYet;
        data.playerLocation = this.transform.position; // Tallenna nykyinen sijainti

        return data;
    }

    // Metodi, joka asettaa ladatut tiedot tähän pelaajaobjektiin
    public void ApplyLoadedData(PlayerSaveData loadedData)
    {
        if (loadedData == null)
        {
            Debug.LogWarning("Annettu ladattu data oli null. Ei sovelleta mitään.");
            return;
        }

        this.currentHealth = loadedData.currentHealth;
        this.Level = loadedData.Level;
        this.NumberofItems = loadedData.NumberofItems;
        this.Gold = loadedData.Gold;
        this.playerName = loadedData.playerName;
        this.playerLastname = loadedData.playerLastname;
        this.haveYouDiedYet = loadedData.haveYouDiedYet;
        this.transform.position = loadedData.playerLocation; // Aseta pelaajan sijainti

        Debug.Log("Pelaajan tiedot ja sijainti päivitetty ladatusta datasta.");
    }

    // Esimerkkimetodit tallennukselle ja lataukselle (voit kutsua näitä UI-napeista)
    public void TriggerSave()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData dataToSave = GetCurrentPlayerData();
            SaveManager.Instance.SavePlayerData(dataToSave);
        }
        else
        {
            Debug.LogError("SaveManager.Instance ei löydy scenestä!");
        }
    }

    public void TriggerLoad()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData loadedData = SaveManager.Instance.LoadPlayerData();
            if (loadedData != null)
            {
                ApplyLoadedData(loadedData);
            }
            else
            {
                Debug.Log("Ei löytynyt tallennettua dataa tai lataus epäonnistui.");
                // Tässä voisit halutessasi asettaa oletusarvot tai aloittaa uuden pelin
            }
        }
        else
        {
            Debug.LogError("SaveManager.Instance ei löydy scenestä!");
        }
    }

    // Voit lisätä tämän Startiin, jotta peli yrittää ladata datan automaattisesti käynnistyessään
    // Tai voit laittaa tämän vaikka päävalikkoon "Jatka peliä" -napin taakse
    void Start()
    {
        // Esimerkki: Lataa automaattisesti jos tallennus löytyy.
        // Voit poistaa tämän tai siirtää sen muualle, jos et halua automaattista latausta.
        // if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        // {
        //     Debug.Log("Löytyi tallennusdataa, yritetään ladata...");
        //     TriggerLoad();
        // }
        // else
        // {
        //     Debug.Log("Ei tallennusdataa, aloitetaan oletusarvoilla.");
        // }
    }
}