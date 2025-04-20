using UnityEngine;
using UnityEngine.UI; // Tarvitaan UI-elementteihin
using TMPro; // Jos käytät TextMeshPro kehotteessa

public class LevelExitInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Panel with Descend/Return buttons (initially inactive)")]
    public GameObject choiceUIPanel; // Vedä UI Paneeli tähän Inspectorissa

    [Tooltip("Optional: Text prompt shown when player is near (e.g., '[E] Interact')")]
    public GameObject interactionPrompt; // Vedä TextMeshPro/Text-objekti tähän

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;

    // --- Sisäinen tila ---
    private bool playerIsNear = false;

    // Yleinen viittaus siirtymälogiikkaan (voi olla samassa skriptissä tai erillisessä)
    // Tässä oletetaan, että siirtymälogiikka on erillisessä LevelTransitionManagerissa
    [SerializeField] private LevelTransitionManager transitionManager;


    void Start()
    {
        // Yritä löytää Transition Manager (jos se on erillinen)
        // Voit myös vaatia sen Inspectorissa [SerializeField] avulla
        transitionManager = FindObjectOfType<LevelTransitionManager>();
        if (transitionManager == null)
        {
            Debug.LogError("LevelTransitionManager not found in scene! Exit interactions will not work.");
        }


        // Varmista, että UI on piilossa aluksi
        if (choiceUIPanel != null) choiceUIPanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // Jos pelaaja on lähellä JA painaa interaktio-näppäintä...
        if (playerIsNear && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tarkista onko triggeriin tullut pelaaja (käytä tägiä!)
        if (other.CompareTag("Player")) // Varmista että pelaajallasi on "Player"-tägi!
        {
            playerIsNear = true;
            // Näytä interaktio-kehote, jos se on määritetty
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                Debug.Log("Player entered exit trigger area.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            // Piilota interaktio-kehote
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
                Debug.Log("Player exited exit trigger area.");
            }

            // Voit myös halutessasi piilottaa valintapaneelin, jos pelaaja kävelee pois kesken kaiken
            // if (choiceUIPanel != null && choiceUIPanel.activeSelf)
            // {
            //    choiceUIPanel.SetActive(false);
            //    // Mahdollisesti peruuta pelaajan liikkeen esto tms.
            // }
        }
    }

    // Metodi, joka suoritetaan kun pelaaja interagoi
    // LevelExitInteraction.cs - Interact() metodissa
    private void Interact()
    {
        LevelTransitionManager transitionManager = FindObjectOfType<LevelTransitionManager>(); // Etsi globaali manageri
        if (transitionManager != null)
        {
            Debug.Log("Interaction key pressed. Requesting choice panel.");
            if (interactionPrompt != null) interactionPrompt.SetActive(false);

            // Pyydä globaalia manageria näyttämään paneeli
            transitionManager.ShowChoicePanel(); // <--- Tarvitset tällaisen metodin LevelTransitionManageriin!

            // Pysäytä pelaaja tms.
            // FindObjectOfType<PlayerMovement>()?.DisableMovement();
        }
        else
        {
            Debug.LogError("LevelTransitionManager instance not found in scene!");
        }
    }

    // Voit lisätä tämän metodin, jota kutsutaan kun UI-paneeli suljetaan (napin painalluksen jälkeen)
    public void OnChoiceMade()
    {
       
        // Salli pelaajan liike uudelleen
        // FindObjectOfType<PlayerMovement>()?.EnableMovement();
        // Palauta ajan kulku normaaliksi, jos se pysäytettiin
        // Time.timeScale = 1f;
    }
}