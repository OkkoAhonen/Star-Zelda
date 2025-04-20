using UnityEngine;
using UnityEngine.SceneManagement; // Tarvitaan scenen nimen tarkistukseen

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentDepth { get; private set; } = 1; // Aloitetaan kerroksesta 1
    public int MaxDepth = 3; // M��rit� maksimisyvyys

    // --- Muita Pelaajan Tilaa (Esimerkkej�) ---
    // public int PlayerHealth;
    // public List<Item> PlayerInventory;
    // -----------------------------------------

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // T�RKE��: Pit�� t�m�n objektin hengiss� scene-latauksissa
            Debug.Log("GameManager Initialized - DontDestroyOnLoad");
        }
        else
        {
            // Jos instanssi on jo olemassa (esim. palatessa sceneen), tuhotaan t�m� duplikaatti.
            Debug.LogWarning("Duplicate GameManager detected. Destroying new one.");
            Destroy(gameObject);
            return; // Lopetetaan Awake t�lle duplikaatille
        }

        // Voit ladata tallennetun pelin tilan t�ss�, jos k�yt�t tallennusj�rjestelm��
        // LoadGameData();
    }

    public void IncreaseDepth()
    {
        if (CurrentDepth < MaxDepth)
        {
            CurrentDepth++;
            Debug.Log($"Depth Increased to: {CurrentDepth}");
            // Voit tallentaa pelin tilan t�ss�
            // SaveGameData();
        }
        else
        {
            Debug.LogWarning("Already at Max Depth, cannot increase further.");
            // T�ss� voitaisiin ladata vaikka erityinen "pohjakerros"-scene tai loppuscene
            // SceneManager.LoadScene("FinalFloor");
        }
    }

    public void SetDepth(int newDepth)
    {
        CurrentDepth = Mathf.Clamp(newDepth, 1, MaxDepth);
        Debug.Log($"Depth Set to: {CurrentDepth}");
        // SaveGameData();
    }

    // Metodi, jota kutsutaan esim. p��valikosta uutta peli� aloitettaessa
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
        // Ei v�ltt�m�tt� nollata syvyytt� heti, riippuu logiikasta
        // ResetDepth(); // Tai ehk� ei? Jos halutaan jatkaa samasta syvyydest�?
        Debug.Log("Preparing to return to surface.");
    }

    // Voit lis�t� metodeja tallentamiseen/lataamiseen tarvittaessa
    // void SaveGameData() { ... }
    // void LoadGameData() { ... }
}