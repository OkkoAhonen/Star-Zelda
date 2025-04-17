using UnityEngine;
using UnityEngine.SceneManagement; // Tarvitaan scenen nimen tarkistukseen

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentDepth { get; private set; } = 1; // Aloitetaan kerroksesta 1
    public int MaxDepth = 3; // Määritä maksimisyvyys

    // --- Muita Pelaajan Tilaa (Esimerkkejä) ---
    // public int PlayerHealth;
    // public List<Item> PlayerInventory;
    // -----------------------------------------

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // TÄRKEÄÄ: Pitää tämän objektin hengissä scene-latauksissa
            Debug.Log("GameManager Initialized - DontDestroyOnLoad");
        }
        else
        {
            // Jos instanssi on jo olemassa (esim. palatessa sceneen), tuhotaan tämä duplikaatti.
            Debug.LogWarning("Duplicate GameManager detected. Destroying new one.");
            Destroy(gameObject);
            return; // Lopetetaan Awake tälle duplikaatille
        }

        // Voit ladata tallennetun pelin tilan tässä, jos käytät tallennusjärjestelmää
        // LoadGameData();
    }

    public void IncreaseDepth()
    {
        if (CurrentDepth < MaxDepth)
        {
            CurrentDepth++;
            Debug.Log($"Depth Increased to: {CurrentDepth}");
            // Voit tallentaa pelin tilan tässä
            // SaveGameData();
        }
        else
        {
            Debug.LogWarning("Already at Max Depth, cannot increase further.");
            // Tässä voitaisiin ladata vaikka erityinen "pohjakerros"-scene tai loppuscene
            // SceneManager.LoadScene("FinalFloor");
        }
    }

    public void SetDepth(int newDepth)
    {
        CurrentDepth = Mathf.Clamp(newDepth, 1, MaxDepth);
        Debug.Log($"Depth Set to: {CurrentDepth}");
        // SaveGameData();
    }

    // Metodi, jota kutsutaan esim. päävalikosta uutta peliä aloitettaessa
    public void ResetGame()
    {
        CurrentDepth = 1;
        // Nollaa muut pelaajan tiedot
        // PlayerHealth = defaultHealth;
        // PlayerInventory.Clear();
        Debug.Log("Game Reset initiated (Depth set to 1).");
        // SaveGameData(); // Tallenna nollattu tila
    }

    // Esimerkki: Metodi jota kutsutaan kun palataan pintaan
    public void PrepareForSurface()
    {
        // Ei välttämättä nollata syvyyttä heti, riippuu logiikasta
        // ResetDepth(); // Tai ehkä ei? Jos halutaan jatkaa samasta syvyydestä?
        Debug.Log("Preparing to return to surface.");
    }

    // Voit lisätä metodeja tallentamiseen/lataamiseen tarvittaessa
    // void SaveGameData() { ... }
    // void LoadGameData() { ... }
}