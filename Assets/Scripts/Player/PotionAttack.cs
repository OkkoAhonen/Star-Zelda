using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject potionPrefab;
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private Transform projectilesParent;

    [Header("Charge Settings")]
    [SerializeField] private float maxPotionCharge = 1.5f;
    private float _chargeTimer;

    [Header("Throw Requirements")]
    [SerializeField] private float cooldownTime = 1f;
    private float _cooldownTimer;

    [Header("Potion Arc Parameters")]
    [SerializeField] private float minPotionSpeed = 5f;
    [SerializeField] private float maxPotionSpeed = 15f;
    [SerializeField] private float minArcHeight = 0.5f;
    [SerializeField] private float maxArcHeight = 2f;
    [SerializeField] private float minThrowDistance = 2f;
    [SerializeField] private float maxThrowDistance = 8f;

    [Header("Splash Damage Settings")]
    [SerializeField] private float splashDamage = 10f;
    [SerializeField] private float splashLifespan = 2f;
    [SerializeField] private float splashSizeMultiplier = 1f;

    private LayerMask damageMask;
    private class PotionData
    {
        public GameObject go;
        public Vector3 startPos;
        public Vector3 endPos;
        public float t;
        public float arcH;
    }
    private readonly List<PotionData> _potions = new List<PotionData>();

    private void Awake()
    {
        damageMask = StaticValueManager.DamageEnemiesMask;
        if (projectilesParent == null)
        {
            var go = GameObject.Find("Projectiles");
            projectilesParent = go != null
                ? go.transform
                : new GameObject("Projectiles").transform;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        if (Input.GetMouseButtonDown(0) && _cooldownTimer <= 0f)
            _chargeTimer = 0f;

        if (Input.GetMouseButton(0) && _cooldownTimer <= 0f)
            _chargeTimer = Mathf.Min(_chargeTimer + dt, maxPotionCharge);

        if (Input.GetMouseButtonUp(0) && _cooldownTimer <= 0f)
        {
            ThrowChargedPotion();
            _chargeTimer = 0f;
            _cooldownTimer = cooldownTime;
        }

        for (int i = _potions.Count - 1; i >= 0; i--)
        {
            var p = _potions[i];
            p.t += dt * Mathf.Lerp(minPotionSpeed, maxPotionSpeed, p.t);
            float y = 4f * p.arcH * p.t * (1f - p.t);
            p.go.transform.position = Vector3.Lerp(p.startPos, p.endPos, p.t)
                                       + Vector3.up * y;
            if (p.t >= 1f)
            {
                StartCoroutine(SpawnSplash(p.go.transform.position));
                Destroy(p.go);
                _potions.RemoveAt(i);
            }
        }
    }

    private void ThrowChargedPotion()
    {
        if (potionPrefab == null || firePoint == null) return;

        float chargeRatio = _chargeTimer / maxPotionCharge;
        float maxDist = Mathf.Lerp(minThrowDistance, maxThrowDistance, chargeRatio);

        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseW.z = 0f;
        Vector3 dir = mouseW - firePoint.position;
        float dist = Mathf.Min(dir.magnitude, maxDist);
        Vector3 target = firePoint.position + dir.normalized * dist;

        float arcH = Mathf.Lerp(minArcHeight, maxArcHeight, chargeRatio);

        var go = Instantiate(
            potionPrefab,
            firePoint.position,
            Quaternion.identity,
            projectilesParent
        );

        _potions.Add(new PotionData
        {
            go = go,
            startPos = firePoint.position,
            endPos = target,
            t = 0f,
            arcH = arcH
        });
    }

    private IEnumerator SpawnSplash(Vector3 pos)
    {
        yield return null;

        if (splashPrefab == null) yield break;

        // ? pooled instantiate
        GameObject splashGO = PoolingManager.Instance.GetPooledObject(splashPrefab);
        splashGO.transform.SetParent(projectilesParent, false);
        splashGO.transform.position = pos;
        splashGO.transform.rotation = Quaternion.identity;

        var splashScript = splashGO.GetComponent<SplashArea>();
        splashScript.damage = splashDamage;
        splashScript.lifespan = splashLifespan;
        splashScript.damageMask = damageMask;
        splashScript.sizeMultiplier *= splashSizeMultiplier;
    }
}