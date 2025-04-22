using Unity.VisualScripting;
using UnityEngine;

public class BeholderAI : MonoBehaviour
{
    [Header("AI Timings")] public float waitTime = 3f;
    public float followTime = 2f;
    public int movements = 2;

    [Header("Masks & Settings")] public LayerMask hitMask;
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
        bossAnimation.laser.SetMasks(hitMask, damageMask);
        bossAnimation.SetHitMask(hitMask);
        stateTimer = waitTime;
        // pass masks into Laser
        bossAnimation.laser.SetMasks(hitMask, damageMask);
    }

    private void Update()
    {
        if (!isAlive) return;

        player = GameObject.FindWithTag("Player").transform;

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

                        bool up = Random.value > 0.5f;
                        if (Random.value > 0.5f)
                        {
                            currentAttack = 1;
                            bossAnimation.SetCurrentAttack(currentAttack);
                            bossAnimation.TriggerAttack(up);
                        }
                        else
                        {
                            currentAttack = 3;
                            bossAnimation.SetCurrentAttack(currentAttack);
                            bool bounceRight = Random.value > 0.5f;
                            bossAnimation.TriggerBounceAttack(bounceRight);
                        }
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

    public void Kill()
    {
        isAlive = false;
        bossAnimation.Death();
    }


}