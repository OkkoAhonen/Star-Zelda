using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class Bow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bow;                   // assign your Bow GameObject here
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform particleContainer;     // world?space container for finished particles

    [Header("Charge & Cooldown")]
    [SerializeField] private float maxBowCharge = 1.5f;
    [SerializeField] private float cooldownTime = 0.5f;
    private float _currentCooldown;
    private float _chargeTimer;
    private bool _isCharging;
    private int _formedCount;  // how many orbs have finished fading in

    [Header("UI")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private TMP_Text chargeLabel;
    [SerializeField] private Vector3 sliderOffset;

    [Header("Arrow Stats")]
    [SerializeField] private float minArrowSpeed = 5f;
    [SerializeField] private float maxArrowSpeed = 20f;
    [SerializeField] private float minArrowDamage = 10f;
    [SerializeField] private float maxArrowDamage = 50f;
    [SerializeField] private float minArrowLifeSpan = 2f;
    [SerializeField] private float maxArrowLifeSpan = 5f;
    [SerializeField] private float maxArrowGlow = 1f;
    [SerializeField] private float arrowStuckLifespan = 2f;

    [Header("Big Orb Settings")]
    [SerializeField] private GameObject bigParticlePrefab;
    [SerializeField] private int totalBigParticles = 16;
    [SerializeField] private float bigParticleRadius = 0.3f;
    [SerializeField] private float minBigSpinSpeed = 30f;
    [SerializeField] private float maxBigSpinSpeed = 180f;
    [SerializeField] private float bigParticleAppearSpeed = 2f;

    [Header("Small Particle System")]
    [SerializeField] private ParticleSystem smallPSPrefab;
    private ParticleSystem _smallPSInstance;
    private float _smallPSDefaultSpeed; 
    
    [Header("Small Particle Explosion")]
    [SerializeField] private int smallExplosionCount = 30;
    [SerializeField] private float smallExplosionSpeedMultiplier = 3f;

    // layers
    private LayerMask hitMask, damageMask;

    // big?orb container (children will orbit here)
    private Transform _bigOrbContainer;
    private float _spawnInterval;
    private int _spawnedCount;

    private bool isBowEquipped = false;

    Item equippedItem;

    // track arrows
    private class ArrowData
    {
        public GameObject go;
        public Vector2 dir;
        public float speed;
        public float damage;
        public float life;
        public bool stuck;
        public Light2D light2D;
    }
    private readonly List<ArrowData> _arrows = new List<ArrowData>();
    private Transform _projectilesParent;

    private void Awake()
    {
        // find or create containers
        _projectilesParent = GameObject.Find("Projectiles")?.transform
                             ?? new GameObject("Projectiles").transform;

        _bigOrbContainer = new GameObject("BigOrbContainer").transform;
        // parent the orb container under your bow GameObject
        _bigOrbContainer.SetParent(bow.transform, false);

        // layer masks
        hitMask = StaticValueManager.HitMask;
        damageMask = StaticValueManager.DamageEnemiesMask;

        // compute spawn interval
        _spawnInterval = maxBowCharge / totalBigParticles;

        // UI initialization
        if (chargeSlider) { chargeSlider.minValue = 0f; chargeSlider.maxValue = 1f; chargeSlider.gameObject.SetActive(false); }
        if (chargeLabel) chargeLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        equippedItem = InventoryManager.Instance.GetSelectedItem(false);

        if (equippedItem != null && equippedItem.type == ItemType.Bow)
        {
                bow.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            bow.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        float dt = Time.deltaTime;

        if (_currentCooldown > 0f)
        {
            _currentCooldown -= dt;
        }

        HandleCharging(dt);
        UpdateArrows(dt);

        if (_isCharging && chargeSlider.gameObject.activeSelf)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(bow.transform.position + sliderOffset);
            ((RectTransform)chargeSlider.transform).position = screen;
            ((RectTransform)chargeLabel.transform).position = screen + Vector3.up * 20f;
        }
    }


    private void HandleCharging(float dt)
    {
        if(equippedItem == null) {  return; }   
        if (equippedItem.type != ItemType.Bow) { return; }
        // Begin charge
        if (Input.GetMouseButtonDown(0) && _currentCooldown <= 0f)
        {
            _isCharging = true;
            _chargeTimer = 0f;
            _spawnedCount = 0;

            // clear old orbs
            foreach (Transform t in _bigOrbContainer) Destroy(t.gameObject);
            _bigOrbContainer.localRotation = Quaternion.identity;

            // instantiate small PS under the bow
            if (_smallPSInstance) Destroy(_smallPSInstance.gameObject);
            _smallPSInstance = Instantiate(smallPSPrefab, bow.transform);
            _smallPSInstance.transform.localPosition = Vector3.zero;
            var main = _smallPSInstance.main;
            _smallPSDefaultSpeed = main.startSpeed.constant;
            _smallPSInstance.Play();

            // show UI
            if (chargeSlider) chargeSlider.gameObject.SetActive(true);
            if (chargeLabel) chargeLabel.gameObject.SetActive(true);

            _formedCount = 0;
            if (chargeSlider) chargeSlider.value = 0f;
            if (chargeLabel) chargeLabel.text = $"0/{totalBigParticles}";
        }

        // Charging
        if (_isCharging && Input.GetMouseButton(0))
        {
            _chargeTimer = Mathf.Min(_chargeTimer + dt, maxBowCharge);
            float t = _chargeTimer / maxBowCharge;

            // spawn big orbs
            while (_spawnedCount < totalBigParticles &&
                   _chargeTimer >= (_spawnedCount + 1) * _spawnInterval)
            {
                float angle = 360f * _spawnedCount / totalBigParticles;
                Vector3 localPos = Quaternion.Euler(0, 0, angle) * Vector3.right * bigParticleRadius;
                var orb = Instantiate(bigParticlePrefab, _bigOrbContainer);
                orb.transform.localPosition = localPos;

                // fade?in the orb’s sprite and light
                var sr = orb.GetComponent<SpriteRenderer>();
                var lt = orb.GetComponent<Light2D>();
                float targetLight = lt?.intensity ?? 0f;
                if (lt) lt.intensity = 0f;
                if (sr) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
                StartCoroutine(FadeInOrb(sr, lt, targetLight));

                _spawnedCount++;
            }

            // spin the entire orb container
            float spinSpeed = Mathf.Lerp(minBigSpinSpeed, maxBigSpinSpeed, t);
            _bigOrbContainer.Rotate(0, 0, spinSpeed * dt);
        }

        // Release / fire
        if (_isCharging && Input.GetMouseButtonUp(0))
        {
            FireArrow();
            _isCharging = false;
            _currentCooldown = cooldownTime;

            if (chargeSlider) chargeSlider.gameObject.SetActive(false);
            if (chargeLabel) chargeLabel.gameObject.SetActive(false);

            // small PS: set velocity opposite to arrow direction
            Vector2 dir = -firePoint.right.normalized;
            var vel = _smallPSInstance.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(dir.x * _smallPSDefaultSpeed * 3f);
            vel.y = new ParticleSystem.MinMaxCurve(dir.y * _smallPSDefaultSpeed * 3f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            var main = _smallPSInstance.main;
            main.startSpeed = _smallPSDefaultSpeed * smallExplosionSpeedMultiplier;
            _smallPSInstance.Emit(smallExplosionCount);
            _smallPSInstance.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            _smallPSInstance.transform.SetParent(particleContainer, true);
            _smallPSInstance.transform.localScale = Vector3.one;
            Destroy(_smallPSInstance.gameObject,
                    _smallPSInstance.main.startLifetime.constantMax + 0.5f);

            // absorb big orbs immediately into arrow glow
            var last = _arrows[_arrows.Count - 1];
            float glowPerOrb = maxArrowGlow / totalBigParticles;
            foreach (Transform orb in _bigOrbContainer)
            {
                if (last.light2D != null)
                {
                    last.light2D.intensity = Mathf.Min(
                        maxArrowGlow,
                        last.light2D.intensity + glowPerOrb
                    );
                    last.light2D.color = orb.GetComponent<SpriteRenderer>().color;
                }
                Destroy(orb.gameObject);
            }
        }
    }

    private IEnumerator FadeInOrb(SpriteRenderer sr, Light2D lt, float targetIntensity)
    {
        float alpha = 0f, intensity = 0f;
        while ((sr != null && alpha < 1f) ||
               (lt != null && intensity < targetIntensity))
        {
            float step = bigParticleAppearSpeed * Time.deltaTime;
            if (sr)
            {
                alpha = Mathf.Min(1f, alpha + step);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            }
            if (lt)
            {
                intensity = Mathf.Min(targetIntensity, intensity + step);
                lt.intensity = intensity;
            }
            yield return null;
        }

        // orb is fully “formed” now
        _formedCount++;

        // update UI based on formed orbs
        _formedCount = Mathf.Min(_formedCount + 1, totalBigParticles);
        if (chargeSlider) chargeSlider.value = (float)_formedCount / totalBigParticles;
        if (chargeLabel) chargeLabel.text = $"{_formedCount}/{totalBigParticles}";
    }

    private void FireArrow()
    {
        float t = _chargeTimer / maxBowCharge;
        float speed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, t);
        float dmg = Mathf.Lerp(minArrowDamage, maxArrowDamage, t);
        float life = Mathf.Lerp(minArrowLifeSpan, maxArrowLifeSpan, t);

        Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, -90f);
        var a = Instantiate(arrowPrefab, firePoint.position, rot, _projectilesParent);

        var arrowLight = a.GetComponent<Light2D>();
        if (arrowLight) arrowLight.intensity = 0f;

        Vector2 dir = -firePoint.right.normalized;
        _arrows.Add(new ArrowData
        {
            go = a,
            dir = dir,
            speed = speed,
            damage = dmg,
            life = life,
            stuck = false,
            light2D = arrowLight
        });
    }

    private void UpdateArrows(float dt)
    {
        for (int i = _arrows.Count - 1; i >= 0; i--)
        {
            var arrow = _arrows[i];
            if (arrow.stuck) continue;

            Vector2 oldP = (Vector2)arrow.go.transform.position;
            Vector2 newP = oldP + arrow.dir * arrow.speed * dt;
            var hits = Physics2D.RaycastAll(oldP, arrow.dir, (newP - oldP).magnitude);
            bool didHit = false;

            foreach (var h in hits)
            {

                
                int bit = 1 << h.collider.gameObject.layer;
                if ((bit & hitMask) != 0)
                {
                    arrow.go.transform.position = h.point;
                    arrow.go.transform.SetParent(h.collider.transform, true);
                    arrow.go.transform.localScale = Vector3.one;
                    arrow.stuck = true;
                    Destroy(arrow.go, arrowStuckLifespan);
                    _arrows.RemoveAt(i);
                    didHit = true;
                    break;
                }
                else if ((bit & damageMask) != 0)
                {
                    var hitGO = h.collider.gameObject;
                    

                    dealDamage(hitGO, 30);

                    if (hitGO.name == "SkeletonBoss")
                        hitGO.GetComponent<SkeletonBossAI>().TakeDamage(arrow.damage);
                    Destroy(arrow.go);
                    _arrows.RemoveAt(i);
                    didHit = true;
                    break;
                }
            }

            if (!didHit)
                arrow.go.transform.position = newP;

            if (!arrow.stuck)
            {
                arrow.life -= dt;
                if (arrow.life <= 0f)
                {
                    Destroy(arrow.go);
                    _arrows.RemoveAt(i);
                }
            }
        }
    }

    private void dealDamage(GameObject enemy, float damage)
    {
        switch (enemy.name)
        {
            case "Enemy1":
                MageSkeletonController mageSkeletonController = enemy.GetComponent<MageSkeletonController>();
                mageSkeletonController.TakeDamage((int)damage);
                Debug.Log($"MageSkeleton currenthealth: {mageSkeletonController.currentHealth} / {mageSkeletonController.maxHealth}");
                break;
            case "Enemy2":
                PirateCaptainController pirateCaptainController = enemy.GetComponentInParent<PirateCaptainController>();
                pirateCaptainController.TakeDamage((int)damage);
                Debug.Log($"PirateCaptain currenthealth: {pirateCaptainController.currentHealth} / {pirateCaptainController.maxHealth}");
                break;
            case "Enemy3":
                ImpAI impAi = enemy.GetComponentInParent<ImpAI>();
                impAi.TakeDamage((int)damage); Debug.Log($"IMP currenthealth: {impAi.currentHealth} / {impAi.maxHealth}");

                break;
            case "Enemy4":
                RockyDudeAI rockyDudeAI = enemy.GetComponent<RockyDudeAI>();
                rockyDudeAI.TakeDamage((int)damage);

                break;
            case "Enemy5":
                GoblinAI goblinAI = enemy.GetComponent<GoblinAI>();
                goblinAI.TakeDamage((int)damage);
                break;
        }
    }
}
