using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct SliderSetting
{
    public string key;          // e.g. "MasterVolume", "MouseSpeed"
    public Slider slider;
    public TMP_Text value;        // shows the formatted value
    public float minValue;
    public float maxValue;
    public bool isPercentage;
}

public class MenuManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string defaultPlayScene = "Game";

    [Header("Panels (MainMenu scene)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("Pointer Icon List")]
    [SerializeField] private Button pointerIconToggle;
    [SerializeField] private GameObject pointerIconList;
    [SerializeField] private Image pointerIconPreview;

    [Header("Resolution List")]
    [SerializeField] private Button resolutionToggle;
    [SerializeField] private GameObject resolutionOptionList;
    [SerializeField] private TMP_Text resolutionLabel;
    [SerializeField] private Button applyResolutionButton;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Sliders")]
    [SerializeField] private SliderSetting[] sliderSettings;

    [Header("Fade Transition")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeSpeed = 2f;
    private readonly Vector2 fadeStart = new Vector2(-1920f, 0f);
    private readonly Vector2 fadeEnd = Vector2.zero;

    private bool isPaused = false;
    private string lastNonMenuScene = null;
    private Resolution[] availableResolutions;
    private int selectedResolutionIndex = 0;

    private void Awake()
    {
        // remember last non-menu scene
        string current = SceneManager.GetActiveScene().name;
        if (current != mainMenuSceneName)
            lastNonMenuScene = current;

        // prepare fade panel
        if (fadePanel != null)
        {
            RectTransform rt = fadePanel.rectTransform;
            rt.anchoredPosition = fadeStart;
            fadePanel.color = Color.black;
        }

        // panels visibility
        if (mainMenuPanel != null) mainMenuPanel.SetActive(current == mainMenuSceneName);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);

        InitSettingsUI();

        // skip animations if disabled
        if (FindFirstObjectByType<MenuAnimation>() is MenuAnimation ma && !ma.enabled)
            ma.SkipMenuAnimations();
    }

    private void InitSettingsUI()
    {
        // sliders
        for (int i = 0; i < sliderSettings.Length; i++)
        {
            var ss = sliderSettings[i];
            ss.slider.minValue = 0f;
            ss.slider.maxValue = 1f;
            float saved = PlayerPrefs.GetFloat(ss.key, 1f);
            ss.slider.value = saved;
            ss.value.text = FormatValue(ss, saved);
            ss.slider.onValueChanged.AddListener(norm =>
            {
                ss.value.text = FormatValue(ss, norm);
                PlayerPrefs.SetFloat(ss.key, norm);
            });
        }

        // pointer-icon toggle
        pointerIconToggle.onClick.AddListener(() =>
            pointerIconList.SetActive(!pointerIconList.activeSelf)
        );

        // resolution toggle
        resolutionToggle.onClick.AddListener(() =>
            resolutionOptionList.SetActive(!resolutionOptionList.activeSelf)
        );

        // fullscreen
        fullscreenToggle.onValueChanged.AddListener(isFull =>
            Screen.fullScreen = isFull
        );

        // gather resolutions & set label
        availableResolutions = Screen.resolutions;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var r = availableResolutions[i];
            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height)
            {
                selectedResolutionIndex = i;
                break;
            }
        }
        var cur = availableResolutions[selectedResolutionIndex];
        resolutionLabel.text = $"{cur.width} x {cur.height}";
        applyResolutionButton.onClick.AddListener(ApplyResolution);
    }

    private string FormatValue(SliderSetting ss, float normalized)
    {
        float actual = Mathf.Lerp(ss.minValue, ss.maxValue, normalized);
        if (ss.isPercentage)
        {
            return Mathf.RoundToInt(actual * 100f) + "%";
        }
        return actual.ToString("0.##");
    }

    public void OnPointerIconSelected(int idx)
    {
        Sprite icon = MousePointerManager.Instance.GetIcon(idx);
        pointerIconPreview.sprite = icon;
        pointerIconList.SetActive(false);
        MousePointerManager.Instance.SetIcon(idx);
        PlayerPrefs.SetInt("PointerIcon", idx);
    }

    // called by each button under resolutionOptionList
    public void OnResolutionChosen(int idx)
    {
        selectedResolutionIndex = idx;
        Resolution r = availableResolutions[idx];
        resolutionLabel.text = $"{r.width} x {r.height}";
        resolutionOptionList.SetActive(false);
        PlayerPrefs.SetInt("ResolutionIndex", idx);
    }

    public void ApplyResolution()
    {
        var r = availableResolutions[selectedResolutionIndex];
        Screen.SetResolution(r.width, r.height, fullscreenToggle.isOn);
    }

    public void StartGame()
        => StartCoroutine(FadeAndLoadScene(defaultPlayScene));

    public void ContinueGame()
        => StartCoroutine(FadeAndLoadScene(
            lastNonMenuScene ?? defaultPlayScene));

    public void ToggleSettings()
        => settingsMenuPanel.SetActive(!settingsMenuPanel.activeSelf);

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

    private IEnumerator FadeAndLoadScene(string scene)
    {
        if (scene != mainMenuSceneName)
            lastNonMenuScene = scene;

        if (fadePanel != null)
        {
            float t = 0f;
            var rt = fadePanel.rectTransform;
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
