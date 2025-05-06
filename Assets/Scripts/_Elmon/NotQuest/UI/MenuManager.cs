using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // fade-panel for smooth transitions
    [Header("Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeSpeed = 2f;
    private Vector2 startPos = new Vector2(-1920f, 0f);
    private Vector2 endPos = Vector2.zero;

    // pause menu (in-game) UI root
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    // which scene is our Main Menu?
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Game";

    private bool isPaused = false;

    private void Start()
    {
        // prepare the fade panel off-screen and fully opaque
        if (fadePanel != null)
        {
            fadePanel.rectTransform.anchoredPosition = startPos;
            fadePanel.color = Color.black;
        }
        // ensure pause menu is hidden at launch
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        // Toggle pause with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // --- PAUSE MENU FLOW ---
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    // “Quit To Menu”—fade out and load the MainMenu scene
    public void QuitToMenu()
    {
        // if we’re paused, un-pause so that timeScale is normal when main menu runs
        if (isPaused) Time.timeScale = 1f;
        StartCoroutine(FadeAndLoadScene(mainMenuSceneName));
    }

    // “Play” from MainMenu scene
    public void StartGame()
    {
        StartCoroutine(FadeAndLoadScene(gameSceneName));
    }

    // straightforward quit
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // reuse this for both StartGame and QuitToMenu
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadePanel != null)
        {
            float elapsed = 0f;
            RectTransform rt = fadePanel.rectTransform;

            // slide the panel in from left to right
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed);
                yield return null;
            }

            rt.anchoredPosition = endPos;
        }

        SceneManager.LoadScene(sceneName);
    }
}
