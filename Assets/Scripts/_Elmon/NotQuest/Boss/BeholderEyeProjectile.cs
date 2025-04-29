using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class BeholderEyeProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed along the initial spray direction")]
    public float moveSpeed = 5f;
    [Tooltip("Constant upward speed added on top of moveSpeed")]
    public float waveSpeed = 1f;

    [Header("Damage & Life")]
    [Tooltip("How long before the eye auto-despawns")]
    public float eyeLifespan = 5f;
    [Tooltip("Damage dealt to the player")]
    public int eyeDamage = 1;

    // These get passed in by your spawner (BeholderAI/BeholderAnimation)
    private LayerMask hitMask;
    private LayerMask damageMask;
    private Vector2 direction;

    private Rigidbody2D rb;
    private Animator animator;
    private bool hasHit = false;

    /// Call this immediately after Instantiate.
    public void Initialize(Vector2 dir, LayerMask hitMask, LayerMask damageMask, int damage, float lifespan = 5f)
    {
        this.direction = dir.normalized;
        this.hitMask = hitMask;
        this.damageMask = damageMask;
        this.eyeDamage = damage;
        this.eyeLifespan = lifespan;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // auto?destroy after lifespan
        StartCoroutine(AutoDespawn());
    }

    private IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(eyeLifespan);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (hasHit) return;

        // Move in the spray direction, plus a constant upward wave
        rb.linearVelocity = (direction * moveSpeed) + (Vector2.up * waveSpeed);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        int layer = col.gameObject.layer;
        bool isHitLayer = ((1 << layer) & hitMask) != 0;
        bool isDamageLayer = ((1 << layer) & damageMask) != 0;

        // ignore anything not in either mask
        if (!isHitLayer && !isDamageLayer) return;

        // if it hits something damageable, and it's the player…
        if (isDamageLayer && col.CompareTag("Player"))
        {
            // apply damage
            PlayerStatsManager.instance.TakeDamage(eyeDamage);
        }

        // trigger our hit animation & stop moving
        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("SomethingHit");

        // wait for the "Exit" state in the animator, then destroy
        StartCoroutine(WaitForExitAndDestroy());
    }

    private IEnumerator WaitForExitAndDestroy()
    {
        // spin until the animator’s current state is named "Exit"
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Exit"))
            yield return null;

        Destroy(gameObject);
    }
}