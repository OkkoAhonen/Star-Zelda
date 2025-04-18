using UnityEngine;
using UnityEngine.UI; // Required for Slider

[RequireComponent(typeof(SpriteRenderer))]
public class BowAttack : MonoBehaviour
{
    [Header("Bow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float maxChargeTime = 1.5f;
    //[SerializeField] private float minChargeTime = 0.2f;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] private float maxArrowSpeed = 20f;
    [SerializeField] private float maxArrowDamage = 50f;
    [SerializeField] private float arrowLifespan = 3f;

    [Header("UI")]
    [SerializeField] private Slider chargeSlider; // Slider to show charge progress

    [Header("Debug")]
    [SerializeField] private float currentChargeTime = 0f;
    [SerializeField] private float currentCooldown = 0f;
    [SerializeField] private bool isCharging = false;

    public float CurrentChargeTime => currentChargeTime;
    public float CurrentCooldown => currentCooldown;
    public bool IsCharging => isCharging;

    void Update()
    {
        HandleCooldown();
        HandleInput();
        UpdateSlider();
    }

    void HandleCooldown()
    {
        if (currentCooldown > 0f)
            currentCooldown -= Time.deltaTime;
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Fire1") && currentCooldown <= 0f)
            StartCharging();

        if (Input.GetButton("Fire1") && isCharging)
            Charge();

        if (Input.GetButtonUp("Fire1") && isCharging)
            ReleaseArrow();
    }

    void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
    }

    void Charge()
    {
        currentChargeTime += Time.deltaTime;
        currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);
    }

    void ReleaseArrow()
    {
        isCharging = false;
        
        float chargePercent = currentChargeTime / maxChargeTime;
        float arrowSpeed = chargePercent * maxArrowSpeed;
        float arrowDamage = chargePercent * maxArrowDamage;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Initialize(arrowSpeed, arrowDamage, arrowLifespan);

        currentCooldown = cooldownTime;
        currentChargeTime = 0f; // Reset charge
    }

    void UpdateSlider()
    {
        if (chargeSlider == null)
            return;

        if (isCharging)
            chargeSlider.value = currentChargeTime / maxChargeTime;
        else
            chargeSlider.value = 0f;
    }
}
