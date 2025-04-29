using System.Collections;
using UnityEngine;

public class BeholderAI : MonoBehaviour
{
    [Header("Attack Weights (out of total)")]
    public int laserWeight = 1;
    public int bounceWeight = 1;
    public int circleWeight = 1;  // was otherWeight
    public int playerWeight = 1;  // new fourth option

    [Header("Eye Circle Attack")]
    public float eyeCircleDuration;
    public float eyeCircleSpawnRate;
    public float eyeCircleSpawnSize;

    [Header("Eye Player-Target Attack")]
    public float eyePlayerDuration;
    public float eyePlayerSpawnRate;
    public float eyePlayerSpawnSize;
    [Tooltip("Half-angle of the cone (in degrees) within which eyes are fired at the player")]
    public float eyeAttackRadius;

    [Header("Common Eye Settings")]
    public int eyeDamage;
    public float eyeLifespan;
    public GameObject eyePrefab;

    [Header("AI Timings")]
    public float waitTime = 3f;
    public float followTime = 2f;
    public int movements = 2;

    [Header("Masks & Settings")]
    public LayerMask hitMask;
    public LayerMask damageMask;
    public bool startBounceRight = true;

    [Header("References (do not touch)")]
    public Transform player;
    public BeholderAnimation bossAnimation;
    public BossBase bossBase;

    [HideInInspector] public int currentAttack;
    private int moveCount;

    private enum AIState { Waiting, Following, Attacking }
    private AIState currentState = AIState.Waiting;
    private float stateTimer;
    private bool isAlive = true;

    // cached transform under which eyes should spawn
    private Transform laserHolder;

    private void Start()
    {
        bossAnimation = transform.GetComponent<BeholderAnimation>();
        bossBase = transform.GetComponent<BossBase>();
        // grab the Transform of your laserHolder (parent of the Laser script)
        laserHolder = bossAnimation.laser.transform.parent;

        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        bossAnimation.laser.SetMasks(hitMask, damageMask);
        bossAnimation.SetHitMask(hitMask);

        stateTimer = waitTime;
    }

    private void Update()
    {
        if (!isAlive) return;

        switch (currentState)
        {
            case AIState.Waiting:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    stateTimer = followTime;
                    currentState = AIState.Following;
                }
                break;

            case AIState.Following:
                bossAnimation.FollowTarget(player);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    moveCount++;
                    if (moveCount >= movements)
                    {
                        moveCount = 0;
                        currentState = AIState.Attacking;
                        TriggerWeightedAttack();
                    }
                    else
                    {
                        stateTimer = waitTime;
                        currentState = AIState.Waiting;
                    }
                }
                break;

            case AIState.Attacking:
                // for the circle & player attacks, we let the coroutine run
                // but we instantly return to Following so movement & facing can continue.
                // If you want to lock out movement during these attacks, change this.
                stateTimer = followTime;
                currentState = AIState.Following;
                break;
        }
    }

    private void TriggerWeightedAttack()
    {
        int totalWeight = laserWeight + bounceWeight + circleWeight + playerWeight;
        int roll = Random.Range(1, totalWeight + 1);

        if (roll <= laserWeight)
        {
            currentAttack = 1;
            bossAnimation.SetCurrentAttack(currentAttack);
            bool up = Random.value > 0.5f;
            bossAnimation.TriggerAttack(up);
        }
        else if (roll <= laserWeight + bounceWeight)
        {
            currentAttack = 3;
            bossAnimation.SetCurrentAttack(currentAttack);
            bool bounceRight = Random.value > 0.5f;
            bossAnimation.TriggerBounceAttack(bounceRight);
        }
        else if (roll <= laserWeight + bounceWeight + circleWeight)
        {
            currentAttack = 2;
            bossAnimation.SetCurrentAttack(currentAttack);
            StartCoroutine(EyeCircleAttack());
        }
        else
        {
            currentAttack = 4;
            bossAnimation.SetCurrentAttack(currentAttack);
            StartCoroutine(EyePlayerAttack());
        }
    }

    private IEnumerator EyeCircleAttack()
    {
        float interval = 1f / eyeCircleSpawnRate;
        float elapsed = 0f;

        while (elapsed < eyeCircleDuration)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            SpawnEye(dir, eyeCircleSpawnSize);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private IEnumerator EyePlayerAttack()
    {
        float interval = 1f / eyePlayerSpawnRate;
        float elapsed = 0f;

        while (elapsed < eyePlayerDuration)
        {
            // face the player
            Vector2 toPlayer = ((Vector2)player.position - (Vector2)laserHolder.position).normalized;
            bossBase.FaceDirection(toPlayer);

            // pick a random cone offset
            float halfA = eyeAttackRadius * 0.5f;
            float offsetDeg = Random.Range(-halfA, halfA);
            Vector2 dir = Rotate(toPlayer, offsetDeg);

            SpawnEye(dir, eyePlayerSpawnSize);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void SpawnEye(Vector2 dir, float size)
    {
        var eye = Instantiate(eyePrefab, laserHolder.position, Quaternion.identity);
        eye.transform.localScale = Vector3.one * size;

        var proj = eye.GetComponent<BeholderEyeProjectile>();
        proj.Initialize(
            dir,
            hitMask: hitMask,
            damageMask: damageMask,
            damage: eyeDamage,
            lifespan: eyeLifespan
        );
    }

    // helper to rotate a vector by degrees
    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(
            v.x * c - v.y * s,
            v.x * s + v.y * c
        );
    }

    public void Kill()
    {
        isAlive = false;
        bossAnimation.Death();
    }
}