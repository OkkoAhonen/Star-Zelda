using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Arrow Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arrowPrefab;
    private Transform _projectilesParent;
    private Transform _lastArrowTransform;

    [Header("Charge & Cooldown")]
    [SerializeField] private float maxBowCharge = 1.5f;
    [SerializeField] private float cooldownTime = 0.5f;
    private float _currentCooldown;
    private float _chargeTimer;
    private bool _isCharging;

    [Header("Arrow Stats")]
    [SerializeField] private float minArrowSpeed = 5f;
    [SerializeField] private float maxArrowSpeed = 20f;
    [SerializeField] private float minArrowDamage = 10f;
    [SerializeField] private float maxArrowDamage = 50f;
    [SerializeField] private float minArrowLifeSpan = 2f;
    [SerializeField] private float maxArrowLifeSpan = 5f;

    [Header("Charge Particle Prefabs")]
    [SerializeField] private ParticleSystem chargeBowBigPS;    // prefab only!
    [SerializeField] private ParticleSystem chargeBowSmallPS;  // prefab only!

    // Scene?instances of the above
    private ParticleSystem _bigPS;
    private ParticleSystem _smallPS;
    private ParticleSystem.EmissionModule _bigEmission;
    private ParticleSystem.EmissionModule _smallEmission;

    private void Awake()
    {
        // 1) Find your "Projectiles" container
        var projGO = GameObject.Find("Projectiles");
        if (projGO == null)
            Debug.LogError("Bow: no GameObject named 'Projectiles' found in scene!");
        else
            _projectilesParent = projGO.transform;

        // 2) Instantiate particle?system instances under this Bow
        _bigPS = Instantiate(chargeBowBigPS, transform);
        _smallPS = Instantiate(chargeBowSmallPS, transform);

        // Cache their emission modules
        _bigEmission = _bigPS.emission;
        _smallEmission = _smallPS.emission;

        // Ensure they start stopped
        _bigPS.Stop();
        _smallPS.Stop();
    }

    private void Update()
    {
        if (_currentCooldown > 0f)
            _currentCooldown -= Time.deltaTime;

        HandleChargingAndFiring();
    }

    private void HandleChargingAndFiring()
    {
        // Start charging
        if (Input.GetMouseButtonDown(0) && _currentCooldown <= 0f)
        {
            _isCharging = true;
            _chargeTimer = 0f;
            _bigPS.Play();
            _smallPS.Play();
        }

        // Continue charging
        if (_isCharging && Input.GetMouseButton(0))
        {
            _chargeTimer = Mathf.Min(_chargeTimer + Time.deltaTime, maxBowCharge);
            float t = _chargeTimer / maxBowCharge;
            _bigEmission.rateOverTime = Mathf.Lerp(0f, 16f / maxBowCharge, t);
            _smallEmission.rateOverTime = Mathf.Lerp(5f, 50f / maxBowCharge, t);
        }

        // Fire!
        if (_isCharging && Input.GetMouseButtonUp(0))
        {
            FireArrow();
            _isCharging = false;
            _currentCooldown = cooldownTime;

            // backward burst small sparks
            var smallMain = _smallPS.main;
            smallMain.startSpeed = -smallMain.startSpeed.constant;
            _smallPS.Emit(20);
            _smallPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // detach a big?PS burst onto the arrow
            SpawnBigPSOnArrow();
            _bigPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void FireArrow()
    {
        float t = _chargeTimer / maxBowCharge;
        float speed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, t);
        float damage = Mathf.Lerp(minArrowDamage, maxArrowDamage, t);
        float lifeSpan = Mathf.Lerp(minArrowLifeSpan, maxArrowLifeSpan, t);

        // If your arrow sprite points down by default, use -90°.
        // If it points right, use 0°, etc. Tweak as needed.
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, 180f);

        // Parent under your Projectiles container (if found)
        GameObject go;
        if (_projectilesParent != null)
        {
            go = Instantiate(arrowPrefab, firePoint.position, rot, _projectilesParent);
        }
        else
        {
            go = Instantiate(arrowPrefab, firePoint.position, rot);
        }

        _lastArrowTransform = go.transform;

        // Pass stats
        var arrow = go.GetComponent<Arrow>();
        arrow.speed = speed;
        arrow.damage = damage;
        arrow.arrowLifeSpan = lifeSpan;

        // Transfer glow color from big PS to arrow material
        var bigMain = _bigPS.main;
        if (go.TryGetComponent<SpriteRenderer>(out var sr))
        {
            Color glow = bigMain.startColor.color;
            sr.material.SetColor("_GlowColor", glow);
        }
    }

    private void SpawnBigPSOnArrow()
    {
        if (_lastArrowTransform == null)
            return;

        // Spawn a fresh big?PS prefab at the arrow, parented to it
        var ps = Instantiate(chargeBowBigPS,
                             _lastArrowTransform.position,
                             _lastArrowTransform.rotation,
                             _lastArrowTransform);
        ps.Play();

        // Auto?destroy after its max lifetime + a buffer
        float maxLifetime = ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, maxLifetime + 0.5f);
    }
}
