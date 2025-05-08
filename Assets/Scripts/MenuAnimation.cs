using System.Collections;
using UnityEngine;

[System.Serializable]
public class MenuElement
{
    public RectTransform rect;
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public float moveSpeed = 5f;
    public float delay = 0.1f;
}

public class MenuAnimation : MonoBehaviour
{
    public static MenuAnimation Instance { get; private set; }

    [Header("Intro Elements (play once on Start)")]
    [SerializeField] private MenuElement[] introElements;

    [Header("Panels (animated on demand)")]
    [SerializeField] public MenuElement[] panels;

    private bool[] isAnimating;
    private Coroutine[] coroutines;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // initialize arrays
        int panelCount = panels.Length;
        isAnimating = new bool[panelCount];
        coroutines = new Coroutine[panelCount];

        // place all intro elements at their start
        foreach (MenuElement e in introElements)
        {
            e.rect.anchoredPosition = e.startPosition;
        }
        // place all panels at their start (closed)
        foreach (MenuElement p in panels)
        {
            p.rect.anchoredPosition = p.startPosition;
            p.rect.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // play intro animations
        foreach (MenuElement e in introElements)
        {
            StartCoroutine(Animate(e, false, null, -1));
        }
    }

    public void AnimatePanelIn(int idx, System.Action onComplete = null)
        => StartPanel(idx, false, onComplete);

    public void AnimatePanelOut(int idx, System.Action onComplete = null)
        => StartPanel(idx, true, onComplete);

    private void StartPanel(int idx, bool reverse, System.Action onComplete)
    {
        if (idx < 0 || idx >= panels.Length) return;

        // stop any running animation for this panel
        if (coroutines[idx] != null)
            StopCoroutine(coroutines[idx]);

        // if opening, ensure it's active
        if (!reverse)
            panels[idx].rect.gameObject.SetActive(true);

        coroutines[idx] = StartCoroutine(Animate(panels[idx], reverse, onComplete, idx));
    }

    private IEnumerator Animate(MenuElement e, bool reverse, System.Action onComplete, int idx)
    {
        // optional delay
        yield return new WaitForSecondsRealtime(e.delay);

        Vector2 end = reverse ? e.startPosition : e.targetPosition;

        // mark start
        if (idx >= 0) isAnimating[idx] = true;

        // move towards end at constant speed (no easing)
        while (Vector2.Distance(e.rect.anchoredPosition, end) > 0.01f)
        {
            e.rect.anchoredPosition = Vector2.MoveTowards(
                e.rect.anchoredPosition,
                end,
                e.moveSpeed * Time.unscaledDeltaTime * 100
            );
            yield return null;
        }
        // snap exactly
        e.rect.anchoredPosition = end;

        // clean up
        if (idx >= 0)
        {
            isAnimating[idx] = false;
            coroutines[idx] = null;
            if (reverse)        // if closing, deactivate
                e.rect.gameObject.SetActive(false);
        }

        // callback
        onComplete?.Invoke();
    }

    public bool PanelIsAnimating(int idx)
    {
        return idx >= 0 && idx < isAnimating.Length && isAnimating[idx];
    }

    public void SkipMenuAnimations()
    {
        // intro fully in
        foreach (MenuElement e in introElements)
            e.rect.anchoredPosition = e.targetPosition;

        // panels fully in
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].rect.anchoredPosition = panels[i].targetPosition;
            panels[i].rect.gameObject.SetActive(true);
            if (coroutines[i] != null)
                StopCoroutine(coroutines[i]);
            isAnimating[i] = false;
            coroutines[i] = null;
        }
    }
}