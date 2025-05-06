using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class MenuElement
{
    [Tooltip("UI element to animate")]
    public RectTransform rect;
    [Tooltip("Start position (for inspector preview)")]
    public Vector2 startPosition;
    [Tooltip("Target position when animation completes")]
    public Vector2 targetPosition;
    [Tooltip("Speed of movement for this element")]
    public float moveSpeed = 5f;
    [Tooltip("Delay before this element starts moving")]
    public float delay = 0.1f;
}

public class MenuAnimation : MonoBehaviour
{
    [Header("Elements to Animate")]
    [Tooltip("Assign each UI element with its start, target, speed and delay")]
    [SerializeField] private MenuElement[] elements;

    private bool isSkipping = false;
    private bool[] shouldMove;

    private void Start()
    {
        shouldMove = new bool[elements.Length];
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].rect.anchoredPosition = elements[i].startPosition;
            shouldMove[i] = false;
        }

        StartCoroutine(AnimateSequence());
    }

    private void Update()
    {
        if (isSkipping)
        {
            return;
        }

        for (int i = 0; i < elements.Length; i++)
        {
            if (shouldMove[i])
            {
                RectTransform rt = elements[i].rect;
                rt.anchoredPosition = Vector2.Lerp(
                    rt.anchoredPosition,
                    elements[i].targetPosition,
                    Time.deltaTime * elements[i].moveSpeed
                );
                if (Vector2.Distance(rt.anchoredPosition, elements[i].targetPosition) < 0.05f)
                {
                    rt.anchoredPosition = elements[i].targetPosition;
                    shouldMove[i] = false;
                }
            }
        }
    }

    private IEnumerator AnimateSequence()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            yield return new WaitForSeconds(elements[i].delay);
            shouldMove[i] = true;
        }
    }

    // instantly place all elements at their target positions
    public void SkipMenuAnimations()
    {
        isSkipping = true;

        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].rect.anchoredPosition = elements[i].targetPosition;
        }
    }
}