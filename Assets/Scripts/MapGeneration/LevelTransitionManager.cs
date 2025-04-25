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
    public GameObject choiceUIPanel; // Ved� sama paneeli t�h�n kuin LevelExitInteractioniin

    [Tooltip("Reference to the Descend button (optional, for disabling)")]
    public Button descendButton;

    // Viittaus interaction skriptiin (jos tarvitsee kutsua sen metodeja, esim. OnChoiceMade)
    private LevelExitInteraction exitInteraction;


    void Start()
    {
        
        // Etsi viittaus, jos tarpeen
        exitInteraction = (LevelExitInteraction)FindFirstObjectByType(typeof(LevelExitInteraction));

        // Varmista ett� paneeli on piilossa alussa
        if (choiceUIPanel != null)
        {
            choiceUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Choice UI Panel is not assigned to LevelTransitionManager!");
        }

        // P�ivit� nappien tila heti alussa (jos ollaan jo maksimisyvyydess� jostain syyst�)
        // UpdateDescendButtonState();
    }
    // LevelTransitionManager.cs
    public void ShowChoicePanel()
    {
        if (choiceUIPanel != null)
        {
            choiceUIPanel.SetActive(true);
            UpdateDescendButtonState(); // P�ivit� nappien tila kun paneeli avataan
                                        // Voit kutsua t�ss� my�s OnChoicePanelOpened, jos tarpeen
            OnChoicePanelOpened();
        }
        else
        {
            Debug.LogError("Choice UI Panel is not assigned to LevelTransitionManager!");
        }
    }

    // T�M� METODI LINKITET��N "DescendButton":n OnClick() EVENTTIIN INSPECTORISSA
    public void HandleDescend()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        Debug.Log("Descend button clicked.");

        // Tarkista, voidaanko menn� syvemm�lle
        if (GameManager.Instance.CurrentDepth < GameManager.Instance.MaxDepth)
        {
            // Piilota UI
            if (choiceUIPanel != null) choiceUIPanel.SetActive(false);

            // Kerro GameManagerille ett� menn��n syvemm�lle
            GameManager.Instance.IncreaseDepth();

            // Kerro interaktiokomponentille, ett� valinta tehtiin (jos tarpeen)
            exitInteraction?.OnChoiceMade();

            // Lataa Dungeon scene uudelleen (k�ytt�� uutta syvyytt� GenerateLeveliss�)
            choiceUIPanel.SetActive(false);
            SceneManager.LoadScene(dungeonSceneName);
        }
        else
        {
            Debug.LogWarning("Cannot descend further, already at max depth.");
            // T�ss� voitaisiin n�ytt�� viesti pelaajalle tai deaktovoida nappi
        }

    }

    // T�M� METODI LINKITET��N "ReturnButton":n OnClick() EVENTTIIN INSPECTORISSA
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

        // Kerro interaktiokomponentille, ett� valinta tehtiin (jos tarpeen)
        exitInteraction?.OnChoiceMade();

        choiceUIPanel.SetActive(false);
        // Lataa Surface scene
        SceneManager.LoadScene(surfaceSceneName);
    }

    // Voit kutsua t�t� metodia esim. silloin kun ChoicePanel avataan,
    // jotta "Descend"-nappi disabloidaan, jos ollaan jo maksimisyvyydess�.
    public void UpdateDescendButtonState()
    {
        if (descendButton != null && GameManager.Instance != null)
        {
            descendButton.interactable = (GameManager.Instance.CurrentDepth < GameManager.Instance.MaxDepth);
        }
    }

    // T�m� metodi voidaan kutsua LevelExitInteractionista kun paneeli avataan
    public void OnChoicePanelOpened()
    {
        UpdateDescendButtonState();
        // Muita toimia kun paneeli avataan?
    }
}