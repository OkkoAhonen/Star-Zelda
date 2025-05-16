using UnityEngine;
using System.Collections;

public class ImpAI : MonoBehaviour
{
    // Inspector
    public float detectionDistance = 6f;
    public Vector2 meleeOffset;               // offset from player to stop at
    public float lavaDuration = 2f;
    public float walkSpeed = 2f;
    public float idleBetweenAttacksTime = 1f;
    public int meleeDamage = 1;
    public int lavaDamage = 1;
    public GameObject lavaStream;             // assign child lava stream object here

    public GameObject lavaBlobPrefab;
    public Transform lavaBlobSpawnPoint;
    public float lavaBlobLifespan = 2f;

    public int maxHealth = 5;
    int currentHealth;

    // runtime
    Transform target;
    Animator animator;
    bool isDetected = false;
    bool isBusy = false;
    int attackCounter = 0;
    Animator lavasStreamAnimator;

    void Awake()
    {
        lavasStreamAnimator = lavaStream.GetComponent<Animator>();
        lavasStreamAnimator.enabled = false;
        target = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        lavaStream.SetActive(false);           // ensure off by default
    }

    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, target.position);

        if (!isDetected)
        {
            if (distToPlayer <= detectionDistance)
                isDetected = true;
            else
                return;
        }

        // face player
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

        // compute stop point with offset and facing
        int facing = s.x > 0 ? 1 : -1;
        Vector2 stopPoint = (Vector2)target.position
                             + new Vector2(meleeOffset.x * facing, meleeOffset.y);
        float distToStop = Vector2.Distance(transform.position, stopPoint);

        animator.SetBool("walk", false);

        if (distToStop <= 0.25f)
        {
            attackCounter++;
            if (attackCounter % 3 == 0)
                StartLavaAttack();
            else
                StartMeleeAttack();
        }
        else
        {
            animator.SetBool("walk", true);
            Vector2 dir = (stopPoint - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * walkSpeed * Time.deltaTime);
        }
    }

    void StartMeleeAttack()
    {
        isBusy = true;
        animator.SetBool("meleeAttack", true);
        // end-of-melee clip ? AnimationEvent ? OnMeleeEnd()
    }

    void StartLavaAttack()
    {
        isBusy = true;
        animator.SetBool("lavaAttack", true);
        // end-of-lava clip ? AnimationEvent ? OnLavaEnd()
    }

    public void StartLavaLoop()  // AnimationEvent on prep end
    {
        lavaStream.SetActive(true);
        lavasStreamAnimator.enabled = true;
        StartCoroutine(StartLavaBlob());
        StartCoroutine(LavaDurationRoutine());
    }

    private IEnumerator LavaDurationRoutine()
    {
        yield return new WaitForSeconds(lavaDuration);
        StopLava();
    }

    public IEnumerator StartLavaBlob()
    {
        yield return new WaitForSeconds(0.55f); // 0.55 vaa alottaa hyvään aikaan lätäkön
        GameObject parent = GameObject.Find("Projectiles")
                            ?? new GameObject("Projectiles");

        Vector3 spawnPos = lavaBlobSpawnPoint != null
                           ? lavaBlobSpawnPoint.position
                           : transform.position;

        GameObject blob = Instantiate(
            lavaBlobPrefab,
            spawnPos,
            Quaternion.identity,
            parent.transform
        );

        // attach controller in the same script
        var ctl = blob.AddComponent<BlobController>();
        ctl.Init(lavaBlobLifespan, meleeDamage);
    }

    public void StopLava()
    {
        animator.SetBool("lavaAttack", false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        // Melee damage when in melee attack
        if (animator.GetBool("meleeAttack"))
        {
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
            return;
        }

        // Lava damage when lava is streaming
        if (animator.GetBool("lavaAttack") && lavaStream.activeSelf)
        {
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
        }
    }

    public void OnMeleeEnd()  // AnimationEvent
    {
        animator.SetBool("meleeAttack", false);
        StartCoroutine(ResetBusy());
    }

    public void OnLavaEnd()  // AnimationEvent
    {
        animator.SetBool("lavaAttack", false);
        StartCoroutine(DisableLavaAndReset());
    }

    private IEnumerator DisableLavaAndReset()
    {
        lavasStreamAnimator.SetTrigger("endLavaLoop");
        // allow Animator to enter post-lava state
        yield return null;
        // wait for the current clip length minus a small buffer
        var clips = lavasStreamAnimator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
            yield return new WaitForSeconds(clips[0].clip.length - 0.1f);

        lavasStreamAnimator.enabled = false;
        lavaStream.SetActive(false);
        StartCoroutine(ResetBusy());
    }

    IEnumerator ResetBusy()
    {
        yield return new WaitForSeconds(idleBetweenAttacksTime);
        isBusy = false;
    }

    public void TakeDamage(int amt)
    {
        currentHealth -= amt;

        if (currentHealth <= 0)
        {
            // always die immediately
            animator.SetTrigger("death");
            return;
        }

        // if not already doing lavaAttack, stagger
        if (!animator.GetBool("lavaAttack"))
        {
            animator.SetTrigger("hurt");
            isBusy = true;  // prevent further actions until OnHurtEnd
        }
    }

    public void OnHurtEnd()  // AnimationEvent
    {
        isBusy = false;
    }

    public void Die()
    {
        animator.SetTrigger("death");
        // end-of-death ? AnimationEvent ? OnReallyDead()
    }

    public void OnReallyDead()  // AnimationEvent
    {
        animator.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }

    private class BlobController : MonoBehaviour
    {
        float lifespan;
        int damage;
        Animator anim;

        public void Init(float life, int dmg)
        {
            lifespan = life;
            damage = dmg;
        }

        void Start()
        {
            anim = GetComponent<Animator>();
            StartCoroutine(Lifecycle());
        }

        IEnumerator Lifecycle()
        {
            // live for a bit
            yield return new WaitForSeconds(lifespan);

            // trigger extinguish
            anim.SetTrigger("endLavaLoop");

            // wait out that animation
            yield return null;
            var clips = anim.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                yield return new WaitForSeconds(clips[0].clip.length - 0.1f);

            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
                PlayerStatsManager.instance.TakeDamage(damage);
        }
    }
}
