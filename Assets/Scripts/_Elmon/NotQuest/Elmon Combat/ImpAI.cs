using UnityEngine;
using System.Collections;

public class ImpAI : MonoBehaviour
{
    // Inspector
    public float detectionDistance = 6f;
    public Vector2 meleeOffset;
    public float lavaDuration = 2f;
    public float walkSpeed = 2f;
    public float idleBetweenAttacksTime = 1f;
    public int meleeDamage = 1;
    public int lavaDamage = 1;
    public GameObject lavaStream;

    public GameObject lavaBlobPrefab;
    public Transform lavaBlobSpawnPoint;
    public float lavaBlobLifespan = 2f;

    public int maxHealth = 5;
    public int currentHealth;

    // runtime
    Transform target;
    Animator animator;
    bool isDetected = false;
    bool isBusy = false;
    int attackCounter = 0;
    Animator lavasStreamAnimator;
    LayerMask damageMask;

    void Awake()
    {
        damageMask = StaticValueManager.DamageNonEnemiesMask;
        lavasStreamAnimator = lavaStream.GetComponent<Animator>();
        lavasStreamAnimator.enabled = false;
        target = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        lavaStream.SetActive(false);
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
        s.x = target.position.x < transform.position.x
                ? -Mathf.Abs(s.x)
                : Mathf.Abs(s.x);
        transform.localScale = s;

        if (isBusy)
        {
            animator.SetBool("walk", false);
            return;
        }

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
    }

    void StartLavaAttack()
    {
        isBusy = true;
        animator.SetBool("lavaAttack", true);
    }

    public void StartLavaLoop()  // AnimationEvent
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

    private IEnumerator StartLavaBlob()
    {
        yield return new WaitForSeconds(0.55f);

        GameObject parent = GameObject.Find("Projectiles")
                            ?? new GameObject("Projectiles");

        Vector3 spawnPos = lavaBlobSpawnPoint != null
                           ? lavaBlobSpawnPoint.position
                           : transform.position;

        // ? pooled instantiate
        GameObject blob = PoolingManager.Instance.GetPooledObject(lavaBlobPrefab);
        blob.transform.SetParent(parent.transform, false);
        blob.transform.position = spawnPos;
        blob.transform.rotation = Quaternion.identity;

        var splashScript = blob.GetComponent<SplashArea>();
        splashScript.damage = lavaDamage;
        splashScript.lifespan = lavaBlobLifespan;
        splashScript.damageMask = damageMask;
        splashScript.hasEndAnimation = true;
    }

    public void StopLava() => animator.SetBool("lavaAttack", false);

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (animator.GetBool("meleeAttack"))
        {
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
            return;
        }

        if (animator.GetBool("lavaAttack") && lavaStream.activeSelf)
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
    }

    public void OnMeleeEnd()
    {
        animator.SetBool("meleeAttack", false);
        StartCoroutine(ResetBusy());
    }

    public void OnLavaEnd()
    {
        animator.SetBool("lavaAttack", false);
        StartCoroutine(DisableLavaAndReset());
    }

    private IEnumerator DisableLavaAndReset()
    {
        lavasStreamAnimator.SetTrigger("endLavaLoop");
        yield return null;
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
            animator.SetTrigger("death");
            return;
        }
        if (!animator.GetBool("lavaAttack"))
        {
            animator.SetTrigger("hurt");
            isBusy = true;
        }
    }

    public void OnHurtEnd() => isBusy = false;

    public void Die()
    {
        animator.SetTrigger("death");
    }

    public void OnReallyDead()
    {
        animator.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}