using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Tarvitaan Button-viittauksia varten

public class LevelTransitionManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string dungeonSceneName = "DungeonLevel";
    public string surfaceSceneName = "SurfaceHub";

    [Header("UI References")]
    [Tooltip("The parent UI Panel containing the choice buttons")]
    public GameObject choiceUIPanel; // Vedä sama paneeli tähän kuin LevelExitInteractioniin

    [Tooltip("Reference to the Descend button (optional, for disabling)")]
    public Button descendButton;

    // Viittaus interaction skriptiin (jos tarvitsee kutsua sen metodeja, esim. OnChoiceMade)
    private LevelExitInteraction exitInteraction;


    void Start()
    {
        
        // Etsi viittaus, jos tarpeen
        exitInteraction = (LevelExitInteraction)FindFirstObjectByType(typeof(LevelExitInteraction));

        // Varmista että paneeli on piilossa alussa
        if (choiceUIPanel != null)
        {
            choiceUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Choice UI Panel is not assigned to LevelTransitionManager!");
        }

        // Päivitä nappien tila heti alussa (jos ollaan jo maksimisyvyydessä jostain syystä)
        // UpdateDescendButtonState();
    }
    // LevelTransitionManager.cs
    public void ShowChoicePanel()
    {
        if (choiceUIPanel != null)
        {
            choiceUIPanel.SetActive(true);
            UpdateDescendButtonState(); // Päivitä nappien tila kun paneeli avataan
                                        // Voit kutsua tässä myös OnChoicePanelOpened, jos tarpeen
            OnChoicePanelOpened();
        }
        else
        {
            Debug.LogError("Choice UI Panel is not assigned to LevelTransitionManager!");
        }
    }

    // TÄMÄ METODI LINKITETÄÄN "DescendButton":n OnClick() EVENTTIIN INSPECTORISSA
    public void HandleDescend()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        Debug.Log("Descend button clicked.");

        // Tarkista, voidaanko mennä syvemmälle
        if (GameManager.Instance.CurrentDepth < GameManager.Instance.MaxDepth)
        {
            // Piilota UI
            if (choiceUIPanel != null) choiceUIPanel.SetActive(false);

            // Kerro GameManagerille että mennään syvemmälle
            GameManager.Instance.IncreaseDepth();

            // Kerro interaktiokomponentille, että valinta tehtiin (jos tarpeen)
            exitInteraction?.OnChoiceMade();

            // Lataa Dungeon scene uudelleen (käyttää uutta syvyyttä GenerateLevelissä)
            choiceUIPanel.SetActive(false);
            SceneManager.LoadScene(dungeonSceneName);
        }
        else
        {
            Debug.LogWarning("Cannot descend further, already at max depth.");
            // Tässä voitaisiin näyttää viesti pelaajalle tai deaktovoida nappi
        }

    }

    // TÄMÄ METODI LINKITETÄÄN "ReturnButton":n OnClick() EVENTTIIN INSPECTORISSA
    public void HandleReturnToSurface()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        Debug.Log("Return to Surface button clicked.");

        // Piilota UI
        if (choiceUIPanel != null) choiceUIPanel.SetActive(false);

        // Valmistele GameManager pintaan paluuta varten (esim. nollaa syvyys JOS halutaan)
        GameManager.Instance.PrepareForSurface(); // Tai GameManager.Instance.SetDepth(0) tms.

        // Kerro interaktiokomponentille, että valinta tehtiin (jos tarpeen)
        exitInteraction?.OnChoiceMade();

        choiceUIPanel.SetActive(false);
        // Lataa Surface scene
        SceneManager.LoadScene(surfaceSceneName);
    }

    // Voit kutsua tätä metodia esim. silloin kun ChoicePanel avataan,
    // jotta "Descend"-nappi disabloidaan, jos ollaan jo maksimisyvyydessä.
    public void UpdateDescendButtonState()
    {
        if (descendButton != null && GameManager.Instance != null)
        {
            descendButton.interactable = (GameManager.Instance.CurrentDepth < GameManager.Instance.MaxDepth);
        }
    }

    // Tämä metodi voidaan kutsua LevelExitInteractionista kun paneeli avataan
    public void OnChoicePanelOpened()
    {
        UpdateDescendButtonState();
        // Muita toimia kun paneeli avataan?
    }
}