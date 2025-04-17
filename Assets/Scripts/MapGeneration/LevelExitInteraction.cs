using UnityEngine;
using UnityEngine.UI; // Tarvitaan UI-elementteihin
using TMPro; // Jos k�yt�t TextMeshPro kehotteessa

public class LevelExitInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Panel with Descend/Return buttons (initially inactive)")]
    public GameObject choiceUIPanel; // Ved� UI Paneeli t�h�n Inspectorissa

    [Tooltip("Optional: Text prompt shown when player is near (e.g., '[E] Interact')")]
    public GameObject interactionPrompt; // Ved� TextMeshPro/Text-objekti t�h�n

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;

    // --- Sis�inen tila ---
    private bool playerIsNear = false;

    // Yleinen viittaus siirtym�logiikkaan (voi olla samassa skriptiss� tai erillisess�)
    // T�ss� oletetaan, ett� siirtym�logiikka on erillisess� LevelTransitionManagerissa
    [SerializeField] private LevelTransitionManager transitionManager;


    void Start()
    {
        // Yrit� l�yt�� Transition Manager (jos se on erillinen)
        // Voit my�s vaatia sen Inspectorissa [SerializeField] avulla
        transitionManager = FindObjectOfType<LevelTransitionManager>();
        if (transitionManager == null)
        {
            Debug.LogError("LevelTransitionManager not found in scene! Exit interactions will not work.");
        }


        // Varmista, ett� UI on piilossa aluksi
        if (choiceUIPanel != null) choiceUIPanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // Jos pelaaja on l�hell� JA painaa interaktio-n�pp�int�...
        if (playerIsNear && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tarkista onko triggeriin tullut pelaaja (k�yt� t�gi�!)
        if (other.CompareTag("Player")) // Varmista ett� pelaajallasi on "Player"-t�gi!
        {
            playerIsNear = true;
            // N�yt� interaktio-kehote, jos se on m��ritetty
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

            // Voit my�s halutessasi piilottaa valintapaneelin, jos pelaaja k�velee pois kesken kaiken
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

            // Pyyd� globaalia manageria n�ytt�m��n paneeli
            transitionManager.ShowChoicePanel(); // <--- Tarvitset t�llaisen metodin LevelTransitionManageriin!

            // Pys�yt� pelaaja tms.
            // FindObjectOfType<PlayerMovement>()?.DisableMovement();
        }
        else
        {
            Debug.LogError("LevelTransitionManager instance not found in scene!");
        }
    }

    // Voit lis�t� t�m�n metodin, jota kutsutaan kun UI-paneeli suljetaan (napin painalluksen j�lkeen)
    public void OnChoiceMade()
    {
       
        // Salli pelaajan liike uudelleen
        // FindObjectOfType<PlayerMovement>()?.EnableMovement();
        // Palauta ajan kulku normaaliksi, jos se pys�ytettiin
        // Time.timeScale = 1f;
    }
}