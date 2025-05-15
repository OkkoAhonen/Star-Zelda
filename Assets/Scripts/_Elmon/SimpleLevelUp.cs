using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach this script to a GameObject with a Canvas in the scene
public class LevelUpVisual : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text leveledTo;
    public TMP_Text leveledToShadow;
    public GameObject everything;

    [Header("Display Settings")]
    public float displayDuration = 2.5f;
    public float spinDegrees = 360f;
    public float spinSpeed = 360f; // Degrees per second

    private Coroutine displayCoroutine;

    private void Awake()
    {
        everything.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventsManager.instance.playerEvents.onPlayerLevelChangeTo += HandleLevelChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.playerEvents.onPlayerLevelChangeTo -= HandleLevelChange;
    }

    private void HandleLevelChange(int newLevel)
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(ShowLevelChange(newLevel - 1, newLevel));
    }

    private IEnumerator ShowLevelChange(int fromLevel, int toLevel)
    {
        everything.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        leveledTo.text = string.Format("Level {0}  ->  Level {1}", fromLevel, toLevel);
        leveledToShadow.text = leveledTo.text;

        everything.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        // Spin animation
        float rotated = 0f;
        while (rotated < spinDegrees)
        {
            float rotateStep = spinSpeed * Time.deltaTime;
            everything.transform.Rotate(Vector3.forward, rotateStep);
            rotated += rotateStep;
            yield return null;
        }

        // Reset rotation to original (optional, comment if not needed)
        everything.transform.rotation = Quaternion.identity;

        everything.SetActive(false);
    }
}
