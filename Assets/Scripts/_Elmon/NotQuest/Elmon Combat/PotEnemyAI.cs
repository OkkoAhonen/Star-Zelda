using UnityEngine;
using System.Collections;

public class PotEnemyAI : MonoBehaviour
{
    // Inspector settings
    public Vector2 meleeOffset;                // offset from player to stop and melee
    public float activationRange = 4f;
    public float meleeWalkRange = 5f;
    public float attack2Range = 4f;
    public float walkSpeed = 2f;
    public float idleTimeAfterReveal = 1f; 
    public float idleBetweenAttacksTime = 1f;
    public int meleeDamage = 1;
    public int rangedDamage = 2;
    public float projectileArcHeight = 1f;
    public float projectileArcSpeed = 1f;
    public GameObject projectilePrefab;
    public Sprite initialSprite;
    public Transform projectileSpawnPoint;

    // runtime
    int nonEnemyMask;
    Transform target;
    Animator animator;
    SpriteRenderer spriteRenderer;
    bool isRevealed = false;
    bool isBusy = false;

    void Awake()
    {
        nonEnemyMask = StaticValueManager.DamageNonEnemiesMask;
        target = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator.enabled = false;  // stays off until activation
        spriteRenderer.sprite = initialSprite;
    }

    void Update()
    {
        if (!isRevealed)
        {
            // activation check
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position,
                activationRange,
                nonEnemyMask
            );
            if (hit != null && hit.CompareTag("Player"))
                ActivatePot();
            return;
        }

        // flip to face player
        Vector3 s = transform.localScale;
        s.x = (target.position.x < transform.position.x)
                ? -Mathf.Abs(s.x)
                : Mathf.Abs(s.x);
        transform.localScale = s;

        if (isBusy)
        {
            animator.SetBool("walk", false);
            return;
        }

        int facing = transform.localScale.x > 0 ? 1 : -1;
        Vector2 meleePoint = (Vector2)target.position + new Vector2(meleeOffset.x * facing, meleeOffset.y);

        float distToMelee = Vector2.Distance(transform.position, meleePoint);
        float distToPlayer = Vector2.Distance(transform.position, target.position);

        // clear walk flag
        animator.SetBool("walk", false);

        if (distToMelee <= 0.1f)
        {
            StartMeleeAttack();
        }
        else if (distToPlayer <= attack2Range && distToPlayer > meleeWalkRange)
        {
            StartRangedAttack();        // now uses method that sets isBusy = true
        }
        else
        {
            // walk toward the meleePoint if within meleeWalkRange,
            // or toward the raw player if beyond attack2Range:
            Vector2 dest = (distToPlayer > attack2Range)
                           ? (Vector2)target.position
                           : meleePoint;

            animator.SetBool("walk", true);
            Vector2 dir = (dest - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * walkSpeed * Time.deltaTime);
        }
    }

    public void ActivatePot()
    {
        if (isRevealed) return;
        animator.enabled = true;
    }

    public void PotRevealed()
    {
        StartCoroutine(PostRevealIdle());
    }

    IEnumerator PostRevealIdle()
    {
        yield return new WaitForSeconds(idleTimeAfterReveal);
        isRevealed = true;
    }

    // -------------------
    // Melee
    // -------------------
    void StartMeleeAttack()
    {
        isBusy = true;
        animator.SetBool("meleeAttack", true);
        // your animation should enable the pot's melee collider;
        // use OnTriggerEnter2D below to detect hits.
        // at end of clip: call OnMeleeEnd() via AnimationEvent
    }

    public void OnMeleeEnd()  // AnimationEvent
    {
        animator.SetBool("meleeAttack", false);
        StartCoroutine(AttackCooldown());
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // pot's melee collider hits player during meleeAttack
        if (animator.GetBool("meleeAttack") && col.CompareTag("Player"))
        {
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
        }
    }

    // -------------------
    // Ranged
    // -------------------
    void StartRangedAttack()
    {
        isBusy = true;
        animator.SetBool("rangedAttack", true);
        // mid-animation: call SpawnProjectile() via AnimationEvent
        // end-animation: call OnRangedEnd()
    }

    public void SpawnProjectile()  // AnimationEvent
    {
        GameObject parent = GameObject.Find("Projectiles")
                            ?? new GameObject("Projectiles");

        Vector3 spawnPos = projectileSpawnPoint != null
                           ? projectileSpawnPoint.position
                           : transform.position;

        GameObject proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.identity,
            parent.transform
        );

        var ctl = proj.AddComponent<ProjectileController>();
        ctl.Init(
            spawnPos,
            target.position,
            rangedDamage,
            projectileArcHeight,
            projectileArcSpeed,
            nonEnemyMask
        );
    }

    public void OnRangedEnd()  // AnimationEvent
    {
        animator.SetBool("rangedAttack", false);
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(idleBetweenAttacksTime);
        isBusy = false;
        // animator auto-transitions to Idle
    }

    // -------------------
    // Damage & Death
    // -------------------
    public void TakeDamage(int amt)
    {
        if (!isRevealed)
        {
            animator.SetTrigger("death");
            return;
        }
        animator.SetTrigger("hurt");
    }

    public void OnHurtEnd()      // AnimationEvent
    {
        isBusy = false;
    }

    public void OnReallyDead()  // AnimationEvent
    {
        animator.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    // -------------------
    // Gizmos
    // -------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attack2Range);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, meleeWalkRange);
    }

    // -------------------
    // Projectile logic
    // -------------------
    private class ProjectileController : MonoBehaviour
    {
        Vector3 startPos, endPos;
        float damage, arcH, arcSp, t;
        int mask;
        Animator anim;
        bool splashing;

        public void Init(Vector3 from, Vector3 to, int dmg, float h, float sp, int m)
        {
            startPos = from;
            endPos = to;
            damage = dmg;
            arcH = h;
            arcSp = sp;
            mask = m;
            t = 0f;
            splashing = false;
        }

        void Start()
        {
            anim = GetComponent<Animator>();
            anim.Play("fly");
        }

        void Update()
        {
            if (splashing) return;

            t += Time.deltaTime * arcSp;
            float y = 4f * arcH * t * (1f - t);
            transform.position = Vector3.Lerp(startPos, endPos, t)
                                 + Vector3.up * y;

            if (t >= 1f)
                BeginSplash();
        }

        void OnTriggerEnter2D(Collider2D c)
        {
            if (splashing) return;
            if (c.CompareTag("Player"))
            {
                Debug.Log("Projectile hit Player");
                PlayerStatsManager.instance.TakeDamage((int)damage);
                BeginSplash();
            }
            else if (((1 << c.gameObject.layer) & mask) == 0)
            {
                BeginSplash();
            }
        }

        void BeginSplash()
        {
            splashing = true;
            anim.SetTrigger("splash");
            StartCoroutine(DestroyAfterSplash());
        }

        private IEnumerator DestroyAfterSplash()
        {
            yield return null;
            var clips = anim.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                yield return new WaitForSeconds(clips[0].clip.length - 0.05f);
            Destroy(gameObject);
        }
    }
}
