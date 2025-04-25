using UnityEngine;

public class BeholderAI : MonoBehaviour
{
    [Header("Attack Weights (out of total)")]
    [Tooltip("e.g. 2 means 2/8 chance for Laser")]
    public int laserWeight = 2;
    [Tooltip("e.g. 5 means 5/8 chance for Bounce")]
    public int bounceWeight = 5;
    [Tooltip("e.g. 1 means 1/8 chance for Other")]
    public int otherWeight = 1;

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

    [HideInInspector] public int currentAttack;
    private int moveCount;

    private enum AIState { Waiting, Following, Attacking }
    private AIState currentState = AIState.Waiting;
    private float stateTimer;
    private bool isAlive = true;

    private void Start()
    {
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
        int totalWeight = laserWeight + bounceWeight + otherWeight;
        int roll = Random.Range(1, totalWeight + 1); // Inclusive max

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
        else
        {
            currentAttack = 2;
            bossAnimation.SetCurrentAttack(currentAttack);
            Debug.Log("Triggering OtherAttack placeholder");
            // bossAnimation.TriggerOtherAttack(); // Add this when ready
        }
    }
}