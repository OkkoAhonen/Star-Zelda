using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuAnimation : MonoBehaviour
{
    public Button[] menuButtons;
    public Vector2[] targetPositions;
    public GameObject gameTitle;
    public Vector2 titleTargetPosition;
    public float moveSpeed = 5f;
    public float delayBetweenButtons = 0.2f;

    private Vector2 buttonStartPosition = new Vector2(-200f, 0f);
    private Vector2 titleStartPosition = new Vector2(0f, 600f);
    private bool[] hasMoved;
    private bool titleHasMoved;
    private bool isSkipping = false;

    void Start()
    {
        if (menuButtons.Length != targetPositions.Length)
            Debug.LogError("Buttons and target positions must match lengths");

        hasMoved = new bool[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
            rt.anchoredPosition = buttonStartPosition;
            hasMoved[i] = false;
        }

        if (gameTitle != null)
        {
            RectTransform tr = gameTitle.GetComponent<RectTransform>();
            tr.anchoredPosition = titleStartPosition;
            titleHasMoved = false;
        }

        StartCoroutine(AnimateMenu());
    }

    void Update()
    {
        if (isSkipping) return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (hasMoved[i])
            {
                RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPositions[i], Time.deltaTime * moveSpeed);
                if (Vector2.Distance(rt.anchoredPosition, targetPositions[i]) < 0.1f)
                {
                    rt.anchoredPosition = targetPositions[i];
                    hasMoved[i] = false;
                }
            }
        }

        if (titleHasMoved && gameTitle != null)
        {
            RectTransform tr = gameTitle.GetComponent<RectTransform>();
            tr.anchoredPosition = Vector2.Lerp(tr.anchoredPosition, titleTargetPosition, Time.deltaTime * moveSpeed);
            if (Vector2.Distance(tr.anchoredPosition, titleTargetPosition) < 0.1f)
                titleHasMoved = false;
        }
    }

    IEnumerator AnimateMenu()
    {
        titleHasMoved = true;
        yield return new WaitForSeconds(delayBetweenButtons);
        for (int i = 0; i < menuButtons.Length; i++)
        {
            yield return new WaitForSeconds(delayBetweenButtons);
            hasMoved[i] = true;
        }
    }

    // skips the animation and jumps elements to their end positions
    public void SkipMenuAnimations()
    {
        isSkipping = true;

        // snap title
        if (gameTitle != null)
        {
            RectTransform tr = gameTitle.GetComponent<RectTransform>();
            tr.anchoredPosition = titleTargetPosition;
        }

        // snap buttons
        for (int i = 0; i < menuButtons.Length; i++)
        {
            RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
            rt.anchoredPosition = targetPositions[i];
        }
    }
}