using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class LevelUpController : MonoBehaviour
{
    [Header("Flash & Freeze")]
    public Image whiteFlashOverlay;          // Full-screen UI Image for white flash
    public float flashFadeDuration = 0.1f;   // Fade in/out duration
    public float flashZoomMultiplier = 1.4f; // Zoom multiplier on flash
    public float outroZoomMultiplier = 1.2f; // Zoom multiplier on second flash

    [Header("Camera Control")]
    public CinemachineVirtualCamera vCam;
    public float baseOrthographicSize = 5f;
    public float zoomOutDuration = 0.5f;
    public AnimationCurve slowZoomCurve;
    public float idleMoveAmplitude = 0.1f;
    public float idleMoveFrequency = 0.5f;

    [Header("Level-Up Text")]
    public RectTransform titleText;          // "Level Up!" text
    public Vector2 titleTargetPos;           // Final anchored position
    public float titleEntryDuration = 0.25f;
    public AnimationCurve titleSmashCurve;

    [Header("Shockwave")]
    public Material shockwaveMaterial;       // Fullscreen ripple shader material
    public float shockwaveDuration = 0.5f;

    [Header("Particles")]
    public GameObject glowParticlePrefab;
    public int particleCount = 20;
    public float particleSpawnRadius = 2f;
    public float particleOutwardSpeed = 1f;
    public float particleConvergeSpeed = 5f;

    [Header("Stats Replenish")]
    public List<Replenishable> replenishItems = new List<Replenishable>();

    [Header("Pop-Up Text Pool")]
    public RectTransform popUpCorner;
    public GameObject popUpTextPrefab;
    public int popUpPoolSize = 15;
    public float popUpDuration = 1f;
    public float popUpStagger = 0.1f;

    [Header("Loading & Pooling")]
    public List<GameObject> preloadAssets;

    // Internal state
    private List<GameObject> popUpPool;
    private List<GameObject> particlePool;
    private float originalTimeScale;
    private Volume postProcessVolume;

    private void Awake()
    {
        originalTimeScale = Time.timeScale;
        postProcessVolume = FindFirstObjectByType<Volume>();
        BuildPools();
        PreloadAssets();
    }

    /// <summary>
    /// Call to trigger the level-up effect.
    /// </summary>
    public void TriggerLevelUp(int xpGained)
    {
        StartCoroutine(LevelUpSequence(xpGained));
    }

    private IEnumerator LevelUpSequence(int xpGained)
    {
        DisablePlayerInput();

        yield return StartCoroutine(FlashAndFreeze(true));
        yield return StartCoroutine(DoSnap());
        yield return StartCoroutine(AnimateTitleEntry());
        yield return StartCoroutine(HandleParticles());
        yield return StartCoroutine(DoReplenish());
        yield return StartCoroutine(ShowPopUps(xpGained));
        yield return StartCoroutine(FlashAndFreeze(false));
        yield return StartCoroutine(ResetCamera());

        EnablePlayerInput();
    }

    #region Phase Coroutines

    private IEnumerator FlashAndFreeze(bool isEntry)
    {
        // Freeze time
        Time.timeScale = 0f;
        // Desaturate
        if (postProcessVolume != null) postProcessVolume.weight = isEntry ? 1f : 0f;
        // Zoom
        float zoomMult = isEntry ? flashZoomMultiplier : outroZoomMultiplier;
        vCam.m_Lens.OrthographicSize = baseOrthographicSize / zoomMult;

        // Flash fade in
        yield return StartCoroutine(FadeOverlay(0f, 1f, flashFadeDuration));
        // Hold briefly
        yield return new WaitForSecondsRealtime(flashFadeDuration);
        // Flash fade out
        yield return StartCoroutine(FadeOverlay(1f, 0f, flashFadeDuration));
    }

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color col = whiteFlashOverlay.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            col.a = Mathf.Lerp(from, to, t);
            whiteFlashOverlay.color = col;
            yield return null;
        }
        col.a = to;
        whiteFlashOverlay.color = col;
    }

    private IEnumerator DoSnap()
    {
        // Camera shake via impulse
        var impulse = vCam.GetComponent<CinemachineImpulseSource>();
        if (impulse != null) impulse.GenerateImpulse();

        // Snap zoom
        float startSize = vCam.m_Lens.OrthographicSize;
        float targetSize = startSize / flashZoomMultiplier;
        float elapsed = 0f;
        while (elapsed < shockwaveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shockwaveDuration);
            vCam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);

            // Shockwave shader
            if (shockwaveMaterial != null)
                shockwaveMaterial.SetFloat("_Radius", t);

            yield return null;
        }
    }

    private IEnumerator AnimateTitleEntry()
    {
        Vector2 startPos = new Vector2(titleTargetPos.x, titleTargetPos.y + Screen.height);
        titleText.anchoredPosition = startPos;
        float elapsed = 0f;
        while (elapsed < titleEntryDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / titleEntryDuration);
            float ease = titleSmashCurve.Evaluate(t);
            titleText.anchoredPosition = Vector2.LerpUnclamped(startPos, titleTargetPos, ease);
            yield return null;
        }
        titleText.anchoredPosition = titleTargetPos;
    }

    private IEnumerator HandleParticles()
    {
        // Activate & spawn outward
        foreach (var p in particlePool)
        {
            p.SetActive(true);
            Vector2 randDir = Random.insideUnitCircle.normalized;
            p.transform.position = transform.position + (Vector3)(randDir * particleSpawnRadius);
            p.GetComponent<ParticleMover>().StartMoving(randDir * particleOutwardSpeed);
        }

        // Wait for snap to finish
        yield return new WaitForSecondsRealtime(shockwaveDuration);

        // Converge
        for (int i = 0; i < particlePool.Count; i++)
        {
            var p = particlePool[i];
            p.GetComponent<ParticleMover>().StopAndConverge(transform.position, particleConvergeSpeed);
            yield return new WaitForSecondsRealtime(0.05f);
        }

        // Cleanup
        yield return new WaitForSecondsRealtime(particleConvergeSpeed / particleConvergeSpeed);
        foreach (var p in particlePool) p.SetActive(false);
    }

    private IEnumerator DoReplenish()
    {
        float elapsed = 0f;
        while (elapsed < shockwaveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shockwaveDuration);
            foreach (var item in replenishItems)
            {
                float startFill = item.current / item.max;
                item.fillBar.fillAmount = Mathf.Lerp(startFill, 1f, t);
            }
            yield return null;
        }
        // Ensure full
        foreach (var item in replenishItems)
        {
            item.fillBar.fillAmount = 1f;
            item.current = item.max;
        }
    }

    private IEnumerator ShowPopUps(int xpGained)
    {
        int idx = 0;
        // XP Popup
        var popup = popUpPool[idx++];
        popup.SetActive(true);
        var tmp = popup.GetComponentInChildren<TMP_Text>();
        tmp.text = "+" + xpGained + " XP";
        popup.GetComponent<PopupMover>().Animate(popUpDuration);
        yield return new WaitForSecondsRealtime(popUpStagger);

        // Additional popups if needed...
        yield return new WaitForSecondsRealtime(popUpDuration);

        // Deactivate all
        foreach (var p in popUpPool) p.SetActive(false);
    }

    private IEnumerator ResetCamera()
    {
        float startSize = vCam.m_Lens.OrthographicSize;
        float elapsed = 0f;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / zoomOutDuration);
            float eval = slowZoomCurve.Evaluate(t);
            vCam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, baseOrthographicSize, eval);
            yield return null;
        }
        vCam.m_Lens.OrthographicSize = baseOrthographicSize;

        // Unfreeze
        Time.timeScale = originalTimeScale;
        if (postProcessVolume != null) postProcessVolume.weight = 0f;
    }

    #endregion

    #region Utilities

    private void BuildPools()
    {
        popUpPool = new List<GameObject>();
        for (int i = 0; i < popUpPoolSize; i++)
        {
            var go = Instantiate(popUpTextPrefab, popUpCorner);
            go.SetActive(false);
            popUpPool.Add(go);
        }

        particlePool = new List<GameObject>();
        for (int i = 0; i < particleCount; i++)
        {
            var go = Instantiate(glowParticlePrefab);
            go.SetActive(false);
            particlePool.Add(go);
        }
    }

    private void PreloadAssets()
    {
        foreach (var asset in preloadAssets)
        {
            // Touch to load into memory
            asset.SetActive(false);
            asset.SetActive(true);
            asset.SetActive(false);
        }
    }

    private void DisablePlayerInput() { /* Implement as needed */ }
    private void EnablePlayerInput() { /* Implement as needed */ }

    private void PreparePools() { /* Reset states if necessary */ }

    // Continuous idle camera movement
    private void LateUpdate()
    {
        float x = (Mathf.PerlinNoise(Time.unscaledTime * idleMoveFrequency, 0f) - 0.5f) * idleMoveAmplitude;
        float y = (Mathf.PerlinNoise(0f, Time.unscaledTime * idleMoveFrequency) - 0.5f) * idleMoveAmplitude;
        var pos = vCam.transform.localPosition;
        vCam.transform.localPosition = new Vector3(x, y, pos.z);
    }

    #endregion
}

/// <summary>
/// Data class for replenishable items.
/// </summary>
[System.Serializable]
public class Replenishable
{
    public Image fillBar;
    public float current;
    public float max;
}

/// <summary>
/// Simple mover for particles; implement pooling-friendly move/stop logic.
/// </summary>
public class ParticleMover : MonoBehaviour
{
    private Vector3 velocity;
    private bool converging;
    private Vector3 convergeTarget;
    private float convergeSpeed;

    public void StartMoving(Vector3 vel)
    {
        velocity = vel;
        converging = false;
    }

    public void StopAndConverge(Vector3 target, float speed)
    {
        converging = true;
        convergeTarget = target;
        convergeSpeed = speed;
    }

    private void Update()
    {
        if (!converging)
        {
            transform.position += velocity * Time.unscaledDeltaTime;
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position, convergeTarget, convergeSpeed * Time.unscaledDeltaTime);
            if (Vector3.Distance(transform.position, convergeTarget) < 0.1f)
                gameObject.SetActive(false);
        }
    }
}

/// <summary>
/// Handles popup text animation (fade/move) then auto-disables.
/// </summary>
public class PopupMover : MonoBehaviour
{
    private float duration;
    private RectTransform rt;
    private CanvasGroup cg;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
    }

    public void Animate(float dur)
    {
        duration = dur;
        StartCoroutine(DoAnimate());
    }

    private IEnumerator DoAnimate()
    {
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, -50f);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            cg.alpha = 1f - t;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
