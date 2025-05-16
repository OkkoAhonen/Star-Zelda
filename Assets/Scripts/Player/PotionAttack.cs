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

    // Internal tracking of flying potions
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
        // find or create the container
        if (projectilesParent == null)
        {
            GameObject go = GameObject.Find("Projectiles");
            projectilesParent = go != null ? go.transform
                                           : new GameObject("Projectiles").transform;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Cooldown
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        // Get equipped item
        Item equipped = InventoryManager.Instance.GetSelectedItem(false);
        bool canFire = equipped != null
                       && equipped.type == ItemType.potion
                       && _chargeTimer >= equipped.potionActivetimer
                       && _cooldownTimer <= 0f;

        // Start charging
        if (Input.GetMouseButtonDown(0) && canFire)
        {
            _chargeTimer = 0f;
        }

        // Charging
        if (Input.GetMouseButton(0) && canFire)
        {
            _chargeTimer = Mathf.Min(_chargeTimer + dt, maxPotionCharge);
        }

        // Release to throw
        if (Input.GetMouseButtonUp(0) && canFire)
        {
            ThrowChargedPotion(equipped);
            _chargeTimer = 0f;
            _cooldownTimer = cooldownTime;
        }

        // Update flying potions
        for (int i = _potions.Count - 1; i >= 0; i--)
        {
            PotionData p = _potions[i];
            p.t += dt * Mathf.Lerp(minPotionSpeed, maxPotionSpeed, p.t);
            float y = 4f * p.arcH * p.t * (1f - p.t);
            p.go.transform.position = Vector3.Lerp(p.startPos, p.endPos, p.t)
                                       + Vector3.up * y;
            if (p.t >= 1f)
            {
                StartCoroutine(SplashAndDestroy(p.go.transform.position));
                Destroy(p.go);
                _potions.RemoveAt(i);
            }
        }
    }

    private void ThrowChargedPotion(Item equipped)
    {
        // Determine charge ratio
        float chargeRatio = _chargeTimer / maxPotionCharge;

        // Compute actual throw distance
        float maxDist = Mathf.Lerp(minThrowDistance, maxThrowDistance, chargeRatio);
        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseW.z = 0f;
        Vector3 dir = (mouseW - firePoint.position);
        float dist = Mathf.Min(dir.magnitude, maxDist);
        Vector3 target = firePoint.position + dir.normalized * dist;

        // Compute arc height for this throw
        float arcH = Mathf.Lerp(minArcHeight, maxArcHeight, chargeRatio);

        // Instantiate potion
        GameObject go = Instantiate(potionPrefab, firePoint.position, Quaternion.identity, projectilesParent);

        _potions.Add(new PotionData
        {
            go = go,
            startPos = firePoint.position,
            endPos = target,
            t = 0f,
            arcH = arcH
        });
    }

    private IEnumerator SplashAndDestroy(Vector3 pos)
    {
        yield return null; // one frame delay if needed
        if (splashPrefab != null)
            Instantiate(splashPrefab, pos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        // draw maximum throw radius while charging
        Gizmos.color = Color.blue;
        float ratio = Application.isPlaying
                      ? (_chargeTimer / maxPotionCharge)
                      : 1f;
        float r = Mathf.Lerp(minThrowDistance, maxThrowDistance, ratio);
        Gizmos.DrawWireSphere(firePoint.position, r);
    }
}
