using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonBossAI : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float healthDifficultyMultiplier = 1f;
    [SerializeField] private float difficultyMultiplier = 1f;

    private float health;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 arenaCenter = Vector2.zero;

    [Header("Patterns")]
    [Tooltip("List your patterns here; they will run in order, looping back.")]
    [SerializeField] private List<SBAttackPattern> patterns;

    // internal flags set by animation events
    private bool attackFinished = false;
    private bool attack3Finished = false;
    private bool hurtFinished = false;

    private bool isStaggered = false;
    private int skipAttacksRemaining = 0;
    private float damageAccumulator = 0f;

    private SBPatternStep currentStep;

    private void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        // compute arena center if you have markers (optional)
        // e.g. arenaCenter = (corner1.position + corner2.position) * 0.5f;
        StartCoroutine(RunPatternLoop());
    }

    private void Update()
    {
        // face player when idle or walking
        var state = animator.GetCurrentAnimatorStateInfo(0);
        bool isIdle = state.IsName("Idle");
        bool isWalk = state.IsName("Walk");
        if ((isIdle || isWalk) && !animator.GetBool("Hurt"))
        {
            spriteRenderer.flipX = (player.position.x < transform.position.x);
        }
    }

    // Called by your damage system
    public void TakeDamage(float dmg)
    {
        health = Mathf.Clamp(health - dmg, 0f, maxHealth);

        // speed up animations at low health
        float hpFrac = health / maxHealth;
        animator.speed = 1f + (1f - hpFrac) * healthDifficultyMultiplier;

        // accumulate for stagger
        damageAccumulator += dmg;
        if (!isStaggered
            && currentStep != null
            && (currentStep.stepType == SBPatternStep.StepType.WalkToPlayer
             || currentStep.stepType == SBPatternStep.StepType.WalkToCenter)
            && currentStep.staggerWhileWalking
            && damageAccumulator >= currentStep.staggerDamageThreshold)
        {
            isStaggered = true;
            skipAttacksRemaining = currentStep.staggerSkipAttacks;
        }

        // trigger hurt if in Idle
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            animator.SetBool("Hurt", true);
        }
    }

    // Animation Event ? end of Attack1 or Attack2 combo
    public void OnAttackEnd()
    {
        attackFinished = true;
    }

    // Animation Event ? end of Attack3
    public void OnAttack3End()
    {
        attack3Finished = true;
    }

    // Animation Event ? end of Hurt
    public void OnHurtEnd()
    {
        hurtFinished = true;
    }

    private IEnumerator RunPatternLoop()
    {
        // loop through all patterns forever
        while (true)
        {
            foreach (var pattern in patterns)
            {
                yield return StartCoroutine(ExecutePattern(pattern));
            }
        }
    }

    private IEnumerator ExecutePattern(SBAttackPattern pattern)
    {
        foreach (var step in pattern.steps)
        {
            // skip attacks if staggered
            if (skipAttacksRemaining > 0
                && (step.stepType == SBPatternStep.StepType.Attack1
                 || step.stepType == SBPatternStep.StepType.ComboAttack2
                 || step.stepType == SBPatternStep.StepType.Attack3))
            {
                skipAttacksRemaining--;
                continue;
            }

            currentStep = step;
            damageAccumulator = 0f;

            switch (step.stepType)
            {
                case SBPatternStep.StepType.WalkToPlayer:
                    yield return StartCoroutine(WalkTo(player.position));
                    break;

                case SBPatternStep.StepType.WalkToCenter:
                    yield return StartCoroutine(WalkTo(arenaCenter));
                    break;

                case SBPatternStep.StepType.Attack1:
                    attackFinished = false;
                    animator.SetTrigger("Attack1");
                    yield return new WaitUntil(() => attackFinished);
                    break;

                case SBPatternStep.StepType.ComboAttack2:
                    attackFinished = false;
                    animator.SetBool("Combo", true);
                    yield return new WaitUntil(() => attackFinished);
                    animator.SetBool("Combo", false);
                    break;

                case SBPatternStep.StepType.Attack3:
                    attack3Finished = false;
                    animator.SetTrigger("Attack3");
                    yield return new WaitForSeconds(step.attack3Duration);
                    // tell animator to end Attack3
                    animator.SetTrigger("EndAttack3");
                    yield return new WaitUntil(() => attack3Finished);
                    break;

                case SBPatternStep.StepType.Idle:
                    float hpFrac = health / maxHealth;
                    float idleTime = step.idleDuration
                        / (difficultyMultiplier + 1f
                           + (1f - hpFrac) * healthDifficultyMultiplier);
                    float timer = 0f;
                    while (timer < idleTime)
                    {
                        if (animator.GetBool("Hurt"))
                        {
                            hurtFinished = false;
                            yield return new WaitUntil(() => hurtFinished);
                            animator.SetBool("Hurt", false);
                        }
                        else
                        {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                    }
                    break;
            }

            // reset stagger once out of walk
            if (isStaggered
                && step.stepType != SBPatternStep.StepType.WalkToPlayer
                && step.stepType != SBPatternStep.StepType.WalkToCenter)
            {
                isStaggered = false;
            }
        }
    }

    private IEnumerator WalkTo(Vector2 target)
    {
        animator.SetBool("Walk", true);
        isStaggered = false;

        while (true)
        {
            if (isStaggered) break;

            Vector2 pos = transform.position;
            if (Vector2.Distance(pos, target) < 0.1f) break;

            Vector2 dir = (target - pos).normalized;
            transform.Translate(dir * speed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Walk", false);
        yield return null;
    }
}