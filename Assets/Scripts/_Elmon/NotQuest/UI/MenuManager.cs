using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    // ----- Configuration -----
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string defaultPlayScene = "Game";

    [Header("Animation Control")]
    [SerializeField] private bool animateMenus = true;

    // ----- Panels -----
    [Header("Panels (assign in MainMenu scene)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    // ----- Settings Controls -----
    [Header("Game Settings UI")]
    [SerializeField] private Slider mouseSpeedSlider;
    [SerializeField] private Slider pointerSizeSlider;
    [SerializeField] private Image pointerIconPreview;
    [SerializeField] private TMP_Dropdown pointerIconDropdown;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button applyResolutionButton;

    [SerializeField] private Slider screenShakeSlider;

    // ----- Fade -----
    [Header("Fade Transition")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeSpeed = 2f;
    private readonly Vector2 fadeStart = new Vector2(-1920f, 0f);
    private readonly Vector2 fadeEnd = Vector2.zero;

    // ----- State -----
    private bool isPaused = false;
    private string lastNonMenuScene = null;
    private Resolution[] availableResolutions;
    private int currentResolutionIndex;

    private void Awake()
    {
        // remember last scene
        string current = SceneManager.GetActiveScene().name;
        if (current != mainMenuSceneName)
            lastNonMenuScene = current;

        // fade panel
        if (fadePanel != null)
        {
            RectTransform rt = fadePanel.rectTransform;
            rt.anchoredPosition = fadeStart;
            fadePanel.color = Color.black;
        }

        // panel visibility
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(current == mainMenuSceneName);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null)
            settingsMenuPanel.SetActive(false);

        // init settings UI
        InitSettingsUI();

        // skip animations
        if (!animateMenus)
        {
            MenuAnimation ma = FindFirstObjectByType<MenuAnimation>();
            if (ma != null) ma.SkipMenuAnimations();
        }
    }

    private void InitSettingsUI()
    {
        // mouse settings
        mouseSpeedSlider.onValueChanged.AddListener(MousePointerManager.Instance.SetSpeed);
        pointerSizeSlider.onValueChanged.AddListener(MousePointerManager.Instance.SetSize);
        pointerIconDropdown.onValueChanged.AddListener(OnPointerIconChanged);

        // audio settings
        masterVolumeSlider.onValueChanged.AddListener(value => AudioListener.volume = value);
        sfxVolumeSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("SFXVolume", value));
        musicVolumeSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("MusicVolume", value));

        // resolution
        availableResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string opt = availableResolutions[i].width + " x " + availableResolutions[i].height;
            options.Add(opt);
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        applyResolutionButton.onClick.AddListener(ApplyResolution);

        fullscreenToggle.onValueChanged.AddListener(isFull => Screen.fullScreen = isFull);

        // screen shake
        screenShakeSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("ScreenShake", value));
    }

    private void OnPointerIconChanged(int idx)
    {
        Sprite icon = MousePointerManager.Instance.GetIcon(idx);
        pointerIconPreview.sprite = icon;
        MousePointerManager.Instance.SetIcon(idx);
    }

    // ----- Main Menu Buttons -----
    public void StartGame()
    {
        StartCoroutine(FadeAndLoadScene(defaultPlayScene));
    }

    public void ContinueGame()
    {
        if (!string.IsNullOrEmpty(lastNonMenuScene))
            StartCoroutine(FadeAndLoadScene(lastNonMenuScene));
        else StartGame();
    }

    public void ToggleSettings()
    {
        if (settingsMenuPanel != null)
            settingsMenuPanel.SetActive(!settingsMenuPanel.activeSelf);
    }

    // ----- Pause Menu -----
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

    public void QuitToMenu()
    {
        if (isPaused) Time.timeScale = 1f;
        StartCoroutine(FadeAndLoadScene(mainMenuSceneName));
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ApplyResolution()
    {
        int idx = resolutionDropdown.value;
        Resolution res = availableResolutions[idx];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
    }

    // ----- Fade & Load -----
    private IEnumerator FadeAndLoadScene(string scene)
    {
        if (scene != mainMenuSceneName)
            lastNonMenuScene = scene;

        if (fadePanel != null)
        {
            float t = 0f;
            RectTransform rt = fadePanel.rectTransform;
            while (t < 1f)
            {
                t += Time.deltaTime * fadeSpeed;
                rt.anchoredPosition = Vector2.Lerp(fadeStart, fadeEnd, t);
                yield return null;
            }
            rt.anchoredPosition = fadeEnd;
        }
        SceneManager.LoadScene(scene);
    }
}