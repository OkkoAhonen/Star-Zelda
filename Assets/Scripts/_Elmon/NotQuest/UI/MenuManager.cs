using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct SliderSetting
{
    public string key;
    public Slider slider;
    public TMP_Text value;
    public float minValue;
    public float maxValue;
    public float defaultValue;
    public bool isPercentage;
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string defaultPlayScene = "Game";

    [Header("Animation Control")]
    [SerializeField] private bool animateMenus = true;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("Pointer Icons")]
    [SerializeField] private Button pointerIconToggle;
    [SerializeField] private GameObject pointerIconList;
    [SerializeField] private Image pointerIconPreview;
    [SerializeField] private Transform pointerIconContent;
    [SerializeField] private GameObject dropDownItemPrefab;

    [Header("Resolution")]
    [SerializeField] private Button resolutionToggle;
    [SerializeField] private GameObject resolutionOptionList;
    [SerializeField] private TMP_Text resolutionLabel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Transform resolutionContent;

    [Header("Sliders")]
    [SerializeField] private SliderSetting[] sliderSettings;

    [Header("Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeSpeed = 2f;
    private readonly Vector2 fadeStart = new Vector2(-1920f, 0f), fadeEnd = Vector2.zero;

    private bool isPaused;
    private string lastNonMenuScene;
    private Resolution[] availableResolutions;
    private int selectedResolutionIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        string current = SceneManager.GetActiveScene().name;
        if (current != mainMenuSceneName)
            lastNonMenuScene = current;

        if (fadePanel != null)
        {
            RectTransform rt = fadePanel.rectTransform;
            rt.anchoredPosition = fadeStart;
            fadePanel.color = Color.black;
        }

        mainMenuPanel?.SetActive(current == mainMenuSceneName);
        pauseMenuPanel?.SetActive(false);
        settingsMenuPanel?.SetActive(false);
    }

    private void Start()
    {
        if (!animateMenus)
            MenuAnimation.Instance.SkipMenuAnimations();

        InitSettingsUI();
    }

    private void InitSettingsUI()
    {
        // sliders
        foreach (var ss in sliderSettings)
        {
            ss.slider.minValue = 0f;
            ss.slider.maxValue = 1f;
            float saved = PlayerPrefs.HasKey(ss.key)
                        ? PlayerPrefs.GetFloat(ss.key)
                        : Mathf.Clamp01((ss.defaultValue - ss.minValue) / (ss.maxValue - ss.minValue));
            ss.slider.value = saved;
            ss.value.text = FormatValue(ss, saved);
            ss.slider.onValueChanged.AddListener(norm =>
            {
                ss.value.text = FormatValue(ss, norm);
                PlayerPrefs.SetFloat(ss.key, norm);
            });
        }

        // pointer icons
        pointerIconToggle.onClick.AddListener(() =>
            pointerIconList.SetActive(!pointerIconList.activeSelf)
        );
        PopulatePointerIconList();

        // resolution
        resolutionToggle.onClick.AddListener(() =>
            resolutionOptionList.SetActive(!resolutionOptionList.activeSelf)
        );
        fullscreenToggle.onValueChanged.AddListener(isF => Screen.fullScreen = isF);
        PopulateResolutionList();

        // show current resolution
        var cr = Screen.currentResolution;
        resolutionLabel.text = $"{cr.width} x {cr.height}";
    }

    private string FormatValue(SliderSetting ss, float norm)
    {
        float actual = Mathf.Lerp(ss.minValue, ss.maxValue, norm);
        return ss.isPercentage
            ? Mathf.RoundToInt(actual) + "%"
            : actual.ToString("0.##");
    }

    private int FindPanelIndex(GameObject panel)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        return System.Array.FindIndex(
            MenuAnimation.Instance.panels,
            e => e.rect == rt
        );
    }

    public void ToggleSettings()
    {
        int idx = FindPanelIndex(settingsMenuPanel);
        if (MenuAnimation.Instance.PanelIsAnimating(idx)) return;

        bool opening = !settingsMenuPanel.activeSelf;
        settingsMenuPanel.SetActive(true);
        if (opening)
            MenuAnimation.Instance.AnimatePanelIn(idx);
        else
            MenuAnimation.Instance.AnimatePanelOut(idx, () => settingsMenuPanel.SetActive(false));
    }

    public void TogglePauseMenu()
    {
        int idx = FindPanelIndex(pauseMenuPanel);
        if (MenuAnimation.Instance.PanelIsAnimating(idx)) return;

        bool opening = !pauseMenuPanel.activeSelf;
        if (opening)
        {
            // pause immediately
            isPaused = true;
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
            MenuAnimation.Instance.AnimatePanelIn(idx);
        }
        else
        {
            // unpause immediately
            isPaused = false;
            Time.timeScale = 1f;
            // hide settings if open
            int sidx = FindPanelIndex(settingsMenuPanel);
            if (settingsMenuPanel.activeSelf && !MenuAnimation.Instance.PanelIsAnimating(sidx))
                MenuAnimation.Instance.AnimatePanelOut(sidx, () => settingsMenuPanel.SetActive(false));

            MenuAnimation.Instance.AnimatePanelOut(idx, () => pauseMenuPanel.SetActive(false));
        }
    }

    private void PopulatePointerIconList()
    {
        var icons = MousePointerManager.Instance.pointerIcons;
        for (int i = 0; i < icons.Length; i++)
        {
            var item = Instantiate(dropDownItemPrefab, pointerIconContent);
            var txt = item.transform.GetChild(0).GetComponent<TMP_Text>();
            var img = item.transform.GetChild(1).GetComponent<Image>();

            if (i == 0)
            {
                txt.text = "Default";
                img.gameObject.SetActive(false);
            }
            else
            {
                txt.gameObject.SetActive(false);
                img.sprite = icons[i];
            }

            int idx = i;
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (idx == 0)
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    pointerIconPreview.gameObject.SetActive(false);
                }
                else
                {
                    Texture2D tex = icons[idx].texture as Texture2D;
                    Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
                    pointerIconPreview.sprite = icons[idx];
                    pointerIconPreview.gameObject.SetActive(true);
                }
                PlayerPrefs.SetInt("PointerIcon", idx);
                pointerIconList.SetActive(false);
            });
        }
    }

    private void PopulateResolutionList()
    {
        availableResolutions = Screen.resolutions;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var r = availableResolutions[i];
            var item = Instantiate(dropDownItemPrefab, resolutionContent);
            var txt = item.transform.GetChild(0).GetComponent<TMP_Text>();
            txt.text = $"{r.width} x {r.height}";
            item.transform.GetChild(1).gameObject.SetActive(false);

            int idx = i;
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedResolutionIndex = idx;
                resolutionLabel.text = txt.text;
                resolutionOptionList.SetActive(false);
            });
        }
    }

    public void ApplyResolution()
    {
        var r = availableResolutions[selectedResolutionIndex];
        Screen.SetResolution(r.width, r.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", selectedResolutionIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    public void StartGame()
    {
        // reset settings off-screen
        int si = FindPanelIndex(settingsMenuPanel);
        if (si >= 0)
            MenuAnimation.Instance.panels[si].rect.anchoredPosition =
                MenuAnimation.Instance.panels[si].startPosition;

        StartCoroutine(FadeAndLoadScene(defaultPlayScene));
    }

    public void ContinueGame()
        => StartCoroutine(FadeAndLoadScene(lastNonMenuScene ?? defaultPlayScene));

    public void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
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
        if (scene != mainMenuSceneName) lastNonMenuScene = scene;
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