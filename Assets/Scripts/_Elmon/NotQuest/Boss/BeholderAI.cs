using System.Collections;
using UnityEngine;

public class BeholderAI : MonoBehaviour
{
    [Header("Attack Weights (out of total)")]
    public int laserWeight = 1;
    public int bounceWeight = 1;
    public int circleWeight = 1;
    public int playerWeight = 1;

    [Header("Eye Circle Attack")]
    public float eyeCircleDuration;
    public float eyeCircleSpawnRate;
    public float eyeCircleSpawnSize;
    public bool eyeCircleMoveWhileShooting;

    [Header("Eye Player-Target Attack")]
    public float eyePlayerDuration;
    public float eyePlayerSpawnRate;
    public float eyePlayerSpawnSize;
    [Tooltip("Half-angle of the cone (in degrees) within which eyes are fired at the player")]
    public float eyeAttackRadius;
    public bool eyePlayerMoveWhileShooting;

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

    [Header("References")]
    public Transform player;
    public Transform playerOffset;
    public BeholderAnimation bossAnimation;
    private BossBase bossBase;
    public Transform projectiles; // parent for spawned eyes

    private enum AIState { Waiting, Following, Attacking }
    private AIState currentState;
    private float stateTimer;
    private int moveCount;
    private bool isAlive;
    private bool isDoingEyeAttack;

    private Transform laserHolder;

    private void Start()
    {
        bossAnimation = GetComponent<BeholderAnimation>();
        bossBase = GetComponent<BossBase>();
        laserHolder = bossAnimation.laserHolder;

        if (player == null)
            player = GameObject.FindWithTag("Player").transform;
        if (playerOffset == null)
            playerOffset = player.Find("playerOffset");

            bossAnimation.laser.SetMasks(hitMask, damageMask);
        bossAnimation.SetHitMask(hitMask);

        currentState = AIState.Waiting;
        stateTimer = waitTime;
        isAlive = true;
    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

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
                bossAnimation.FollowTarget(playerOffset);
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
                if (isDoingEyeAttack)
                {
                    return;
                }
                if (!bossAnimation.IsAttackInProgress())
                {
                    stateTimer = followTime;
                    currentState = AIState.Following;
                }
                break;
        }
    }

    private void TriggerWeightedAttack()
    {
        int totalWeight = laserWeight + bounceWeight + circleWeight + playerWeight;
        int roll = Random.Range(1, totalWeight + 1);

        if (roll <= laserWeight)
        {
            currentState = AIState.Attacking;
            bossAnimation.SetCurrentAttack(1);
            bossAnimation.TriggerAttack(Random.value > 0.5f);
        }
        else if (roll <= laserWeight + bounceWeight)
        {
            currentState = AIState.Attacking;
            bossAnimation.SetCurrentAttack(3);
            bossAnimation.TriggerBounceAttack(Random.value > 0.5f);
        }
        else if (roll <= laserWeight + bounceWeight + circleWeight)
        {
            currentState = AIState.Attacking;
            bossAnimation.SetCurrentAttack(2);
            bossBase.SetAttackState(true);
            isDoingEyeAttack = true;
            StartCoroutine(EyeCircleAttack());
        }
        else
        {
            currentState = AIState.Attacking;
            bossAnimation.SetCurrentAttack(4);
            bossBase.SetAttackState(true);
            isDoingEyeAttack = true;
            StartCoroutine(EyePlayerAttack());
        }
    }

    private IEnumerator EyeCircleAttack()
    {
        float interval = 1f / eyeCircleSpawnRate;
        float elapsed = 0f;

        while (elapsed < eyeCircleDuration)
        {
            if (eyeCircleMoveWhileShooting)
            {
                bossAnimation.FollowTarget(playerOffset);
            }

            Vector2 direction = Random.insideUnitCircle.normalized;
            SpawnEyeAt(direction, eyeCircleSpawnSize, laserHolder.position);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        bossBase.SetAttackState(false);
        isDoingEyeAttack = false;
    }

    private IEnumerator EyePlayerAttack()
    {
        Vector2[] offsets = bossAnimation.laserOffsets;
        float interval = 1f / eyePlayerSpawnRate;
        float elapsed = 0f;

        while (elapsed < eyePlayerDuration)
        {
            Vector3 offsetPosition = playerOffset.position;
            Vector2 toTarget = ((Vector2)offsetPosition - (Vector2)laserHolder.position).normalized;
            bossBase.FaceDirection(toTarget);
            if (eyePlayerMoveWhileShooting)
            {
                bossAnimation.FollowTarget(playerOffset);
            }

            foreach (Vector2 off in offsets)
            {
                Vector3 worldOff = laserHolder.TransformVector(off);
                Vector3 spawnPos = laserHolder.position + worldOff;
                Vector2 direction = ((Vector2)offsetPosition - (Vector2)spawnPos).normalized;
                SpawnEyeAt(direction, eyePlayerSpawnSize, spawnPos);
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        bossBase.SetAttackState(false);
        isDoingEyeAttack = false;
    }

    private void SpawnEyeAt(Vector2 direction, float size, Vector3 position)
    {
        GameObject eye = Instantiate(eyePrefab, position, Quaternion.identity, projectiles);
        float bossScale = bossBase.transform.lossyScale.x;
        eye.transform.localScale = Vector3.one * size * bossScale;

        BeholderEyeProjectile proj = eye.GetComponent<BeholderEyeProjectile>();
        proj.Initialize(
            direction,
            hitMask: hitMask,
            damageMask: damageMask,
            damage: eyeDamage,
            lifespan: eyeLifespan
        );
    }
}
