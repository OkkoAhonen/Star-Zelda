using UnityEngine;
using System.Collections;

public class GoblinAI : MonoBehaviour
{
    // Inspector
    public float detectionDistance = 6f;
    public Vector2 meleeOffset;               // side-offset to stop at
    public float walkSpeed = 2f;
    public float idleBetweenAttacksTime = 1f;
    public float distanceForCombo = 1f;
    public int meleeDamage = 1;

    // runtime
    Transform target;
    Animator animator;
    bool isDetected = false;
    bool isBusy = false;

    public int maxHealth = 70;
    public int currentHealth;

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, target.position);

        // detect once
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

        // compute melee stop-point
        int facing = s.x > 0 ? 1 : -1;
        Vector2 meleePoint = (Vector2)target.position
                             + new Vector2(meleeOffset.x * facing, meleeOffset.y);
        float distToMelee = Vector2.Distance(transform.position, meleePoint);

        animator.SetBool("walk", false);

        if (distToMelee <= 0.1f)
        {
            StartMelee();
        }
        else
        {
            // always walk until you're at meleePoint
            animator.SetBool("walk", true);
            Vector2 dir = (meleePoint - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * walkSpeed * Time.deltaTime);
        }
    }

    void StartMelee()
    {
        isBusy = true;
        animator.SetBool("meleeAttack", true);
        // AnimationEvent at end of first attack clip ? OnMeleeEnd()
        // AnimationEvent at end of combo clip ? OnComboEnd()
    }

    // called by your melee collider TriggerEnter2D
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && animator.GetBool("meleeAttack"))
            if(PlayerStatsManager.instance != null) {
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
            }
    }

    public void OnMeleeEnd()   // end-of-first-attack AnimationEvent
    {
        animator.SetBool("meleeAttack", false);
        animator.SetBool("combo", false);
        StartCoroutine(ResetBusy());
    }
    public void TakeDamage(int damage)
    {
        animator.SetTrigger("Hurt");
        currentHealth -= damage;

        if (currentHealth < 0) { Die(); }
    }

    public void CheckCombo()  // end-of-neutral-return AnimationEvent
    {
        // If player still close enough, do combo
        int facing = transform.localScale.x > 0 ? 1 : -1;
        Vector2 meleePoint = (Vector2)target.position
                             + new Vector2(meleeOffset.x * facing, meleeOffset.y);
        bool doCombo = Vector2.Distance(transform.position, meleePoint) <= distanceForCombo;

        if (doCombo)
        {
            animator.SetBool("combo", true);
        }
    }

    IEnumerator ResetBusy()
    {
        yield return new WaitForSeconds(idleBetweenAttacksTime);
        isBusy = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }

    public void Die()
    {
        animator.SetTrigger("death");
        Destroy(gameObject, 1f);
        // at end of Death clip: AnimationEvent ? OnReallyDead()

        AudioManager.instance.PlaySFX("MonsterRoar2");
    }

    public void OnReallyDead()  // AnimationEvent
    {
        animator.enabled = false;
        Destroy(gameObject, 0.1f);
    }
}
