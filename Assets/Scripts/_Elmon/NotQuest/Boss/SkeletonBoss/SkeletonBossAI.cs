using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PatternGroup
{
    [Range(0f, 1f)]
    public float minHealthPercent;
    public float healthDifficultyMultiplier = 1f;
    public List<SBAttackPattern> patterns;
}

public class SkeletonBossAI : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float difficultyMultiplier = 1f;

    [Header("Attack1 Y-Offset")]
    [Tooltip("Vertical offset from player for Attack1")]
    [SerializeField] private float attackYOffset;

    [Header("Arena & Spawn Area")]
    [SerializeField] public Vector2 arenaCenter;
    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;

    [Header("Crystal Prefabs & Parent")]
    [SerializeField] private GameObject normalCrystalPrefab;
    [SerializeField] private GameObject glowingCrystalPrefab;
    [SerializeField] private Transform projectiles;

    [Header("Pattern Groups")]
    [SerializeField] private List<PatternGroup> patternGroups;

    [Header("Attack FX")]
    [SerializeField] private GameObject atk1FX;
    [SerializeField] private GameObject atk2FX;

    [Header("Initial Appearance")]
    [SerializeField] private Sprite initialSprite;

    private float health;
    private float currentHealthDiffMult;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private Vector3 origFX1Pos, origFX2Pos;

    // animation-event flags
    private bool attackFinished, attack3Finished, hurtFinished, deathDone;

    // stagger tracking
    private bool isStaggered;
    private int skipAttacksRemaining;
    private float damageAccumulator;

    private SBPatternStep currentStep;
    private bool isDead;
    private bool canFlip;

    private const float clampGap = 0.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player").transform;

        // prevent Animator from overriding the initial sprite
        animator.enabled = false;
        if (initialSprite != null)
            spriteRenderer.sprite = initialSprite;

        // record FX original positions
        if (atk1FX) origFX1Pos = atk1FX.transform.localPosition;
        if (atk2FX) origFX2Pos = atk2FX.transform.localPosition;

        health = maxHealth;
        patternGroups.Sort((a, b) => b.minHealthPercent.CompareTo(a.minHealthPercent));
    }

    private void Update()
    {
        if (isDead) return;
        if (canFlip)
            spriteRenderer.flipX = (player.position.x < transform.position.x);
    }

    // FX toggles
    public void EnableAtk1FX()
    {
        atk1FX.SetActive(true);
        FlipFX(atk1FX);
        FlipFXPosition(atk1FX, origFX1Pos);
    }

    public void EnableAtk2FX()
    {
        atk2FX.SetActive(true);
        FlipFX(atk2FX);
        FlipFXPosition(atk2FX, origFX2Pos);
    }

    private void FlipFX(GameObject fx)
    {
        var s = fx.transform.localScale;
        s.x = spriteRenderer.flipX ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
        fx.transform.localScale = s;
    }

    private void FlipFXPosition(GameObject fx, Vector3 original)
    {
        var p = original;
        p.x = original.x * (spriteRenderer.flipX ? -1f : 1f);
        fx.transform.localPosition = p;
    }

    public void OnActivated()
    {
        animator.enabled = true;
        StartCoroutine(RunPatternLoop());
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - dmg, 0f, maxHealth);
        if (health <= 0f)
        {
            isDead = true;
            StopAllCoroutines();
            animator.SetTrigger("Dead");
            return;
        }

        float hf = health / maxHealth;
        animator.speed = 1f + (1f - hf) * currentHealthDiffMult;

        damageAccumulator += dmg;
        if (!isStaggered
            && currentStep != null
            && (currentStep.stepType == SBPatternStep.StepType.WalkToPlayer
             || currentStep.stepType == SBPatternStep.StepType.WalkToCenter
             || currentStep.stepType == SBPatternStep.StepType.RandomSpot)
            && currentStep.staggerWhileWalking
            && damageAccumulator >= currentStep.staggerDamageThreshold)
        {
            isStaggered = true;
            skipAttacksRemaining = currentStep.staggerSkipAttacks;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            hurtFinished = false;
            animator.SetTrigger("Hurt");
        }
    }

    // Animation Events
    public void OnAttackEnd() { attackFinished = true; }
    public void OnAttack3Start() { StartCoroutine(SpawnCrystals()); }
    public void OnAttack3End() { attack3Finished = true; }
    public void OnHurtEnd() { hurtFinished = true; }

    public void OnDeathEnd()
    {
        deathDone = true;
        animator.enabled = false;
    }

    public void Resurrect()
    {
        deathDone = false;
        animator.enabled = true;
    }

    private IEnumerator RunPatternLoop()
    {
        while (!isDead)
        {
            float hf = health / maxHealth;
            var group = patternGroups.Find(pg => hf >= pg.minHealthPercent);
            if (group == null) yield break;
            currentHealthDiffMult = group.healthDifficultyMultiplier;

            foreach (var pat in group.patterns)
            {
                if (isDead) yield break;
                yield return ExecutePattern(pat);
            }
        }
        yield return new WaitUntil(() => deathDone);
        // Animator already disabled in OnDeathEnd
    }

    private IEnumerator ExecutePattern(SBAttackPattern pattern)
    {
        foreach (var step in pattern.steps)
        {
            if (skipAttacksRemaining > 0)
            {
                skipAttacksRemaining--;
                continue;
            }

            currentStep = step;
            damageAccumulator = 0f;

            switch (step.stepType)
            {
                case SBPatternStep.StepType.WalkToPlayer:
                    {
                        float side = (player.position.x >= arenaCenter.x) ? -1f : 1f;
                        Vector2 dest = new Vector2(
                            player.position.x + side * Mathf.Abs(transform.localScale.x),
                            player.position.y + attackYOffset
                        );
                        dest.x = Mathf.Clamp(dest.x, spawnAreaMin.x + clampGap, spawnAreaMax.x - clampGap);
                        dest.y = Mathf.Clamp(dest.y, spawnAreaMin.y + clampGap, spawnAreaMax.y - clampGap);
                        yield return WalkTo(dest, step.speedMultiplier);
                    }
                    break;

                case SBPatternStep.StepType.WalkToCenter:
                    yield return WalkTo(arenaCenter, step.speedMultiplier);
                    break;

                case SBPatternStep.StepType.RandomSpot:
                    {
                        Vector2 rnd = new Vector2(
                            Random.Range(spawnAreaMin.x + clampGap, spawnAreaMax.x - clampGap),
                            Random.Range(spawnAreaMin.y + clampGap, spawnAreaMax.y - clampGap)
                        );
                        yield return WalkTo(rnd, step.speedMultiplier);
                    }
                    break;

                case SBPatternStep.StepType.Attack1:
                    attackFinished = false;
                    animator.SetBool("Combo", step.combo);
                    animator.SetTrigger("Attack1");
                    yield return new WaitUntil(() => attackFinished);
                    animator.SetBool("Combo", false);
                    break;

                case SBPatternStep.StepType.Attack3:
                    attack3Finished = false;
                    animator.SetTrigger("Attack3");
                    yield return new WaitUntil(() => attack3Finished);
                    break;

                case SBPatternStep.StepType.Idle:
                    canFlip = false;
                    float hf2 = health / maxHealth;
                    float idleT = step.idleDuration
                               / (difficultyMultiplier + 1f
                                  + (1f - hf2) * currentHealthDiffMult);
                    float timer = 0f;
                    while (timer < idleT)
                    {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
                            yield return new WaitUntil(() => hurtFinished);
                        else
                        {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                    }
                    break;
            }

            if (isStaggered
                && step.stepType != SBPatternStep.StepType.WalkToPlayer
                && step.stepType != SBPatternStep.StepType.WalkToCenter
                && step.stepType != SBPatternStep.StepType.RandomSpot)
            {
                isStaggered = false;
            }
        }
    }

    private IEnumerator WalkTo(Vector2 target, float speedMult)
    {
        animator.SetBool("Walk", true);
        canFlip = true;
        isStaggered = false;

        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            Vector2 next = Vector2.MoveTowards(
                transform.position, target, baseSpeed * speedMult * Time.deltaTime);
            Vector2 dir = (next - (Vector2)transform.position).normalized;
            spriteRenderer.flipX = dir.x < 0f;
            transform.position = next;
            yield return null;
        }

        animator.SetBool("Walk", false);
        canFlip = false;
        yield return null;
    }

    private IEnumerator SpawnCrystals()
    {
        float duration = currentStep.attack3Duration;
        float interval = 1f / currentStep.crystalSpawnRate;
        float elapsed = 0f;
        var used = new List<Vector2>();
        var prefab = (health / maxHealth <= 0f)
                   ? glowingCrystalPrefab
                   : normalCrystalPrefab;
        float r = prefab.GetComponent<CircleCollider2D>().radius;

        while (elapsed < duration)
        {
            Vector2 pos; bool ok; int tries = 0;
            do
            {
                pos = new Vector2(
                    Random.Range(spawnAreaMin.x + clampGap, spawnAreaMax.x - clampGap),
                    Random.Range(spawnAreaMin.y + clampGap, spawnAreaMax.y - clampGap)
                );
                ok = true;
                foreach (var u in used)
                    if (Vector2.Distance(u, pos) < r * 2f)
                    { ok = false; break; }
                tries++;
            } while (!ok && tries < 10);

            var go = Instantiate(prefab, pos, Quaternion.identity, projectiles);
            var cp = go.GetComponent<CrystalProjectile>();
            cp.SetHealth(currentStep.crystalHealth);
            cp.StartLifespan();
            used.Add(pos);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        animator.SetTrigger("EndAttack3");
    }
}