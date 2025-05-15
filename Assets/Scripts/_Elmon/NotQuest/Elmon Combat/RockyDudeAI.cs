using UnityEngine;
using System.Collections;

public class RockyDudeAI : MonoBehaviour
{
    // Inspector
    public float detectionDistance = 6f;
    public Vector2 meleeOffset;
    public float walkSpeed = 2f;
    public float idleBetweenAttacksTime = 1f;
    public int meleeDamage = 1;
    public float slamFollowSpeed = 8f;

    // runtime
    Transform target;
    Animator animator;
    bool isDetected = false;
    bool isBusy = false;
    bool isSlammingFollow = false;
    int attackCounter = 0;

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
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

        if (isSlammingFollow)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * slamFollowSpeed * Time.deltaTime);
            return;
        }

        if (isBusy)
        {
            animator.SetBool("walk", false);
            return;
        }

        // walk toward the player+offset
        int facing = transform.localScale.x > 0 ? 1 : -1;
        // apply flipped X offset
        Vector2 stopPoint = (Vector2)target.position
                            + new Vector2(meleeOffset.x * facing,
                                          meleeOffset.y);
        float distToStop = Vector2.Distance(transform.position, stopPoint);

        animator.SetBool("walk", false);

        if (distToStop <= 0.3f)
        {
            ChooseAttack();
        }
        else
        {
            animator.SetBool("walk", true);
            Vector2 dir = (stopPoint - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * walkSpeed * Time.deltaTime);
        }
    }

    void ChooseAttack()
    {
        attackCounter++;
        if (attackCounter % 3 == 0)
            StartSlamAttack();
        else
            StartMeleeAttack();
    }

    void StartMeleeAttack()
    {
        isBusy = true;
        animator.SetBool("meleeAttack", true);
        // end-of-melee clip ? AnimationEvent ? OnMeleeEnd()
    }

    void StartSlamAttack()
    {
        isBusy = true;
        animator.SetBool("slamAttack", true);
        // AnimationEvent “SlamRiseUp” ? SlamRiseUp()
        // AnimationEvent “SlamRiseStop” ? SlamRiseStop()
        // end-of-slam clip ? AnimationEvent ? OnSlamEnd()
    }

    // called by your melee collider during the meleeAttack animation
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && animator.GetBool("meleeAttack"))
            PlayerStatsManager.instance.TakeDamage(meleeDamage);
    }

    public void OnMeleeEnd()  // AnimationEvent
    {
        animator.SetBool("meleeAttack", false);
        StartCoroutine(ResetBusy());
    }

    public void SlamRiseUp()  // AnimationEvent
    {
        isSlammingFollow = true;
    }

    public void SlamRiseStop()  // AnimationEvent
    {
        isSlammingFollow = false;
    }

    public void OnSlamEnd()  // AnimationEvent
    {
        animator.SetBool("slamAttack", false);
        StartCoroutine(ResetBusy());
    }

    IEnumerator ResetBusy()
    {
        yield return new WaitForSeconds(idleBetweenAttacksTime);
        isBusy = false;
    }

    public void TakeDamage(int amt)
    {
        animator.SetTrigger("hurt");
        // end-of-hurt ? AnimationEvent ? OnHurtEnd()
    }

    public void OnHurtEnd()  // AnimationEvent
    {
        // nothing special; allow movement/attacks again
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
}
