using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonBossAI : MonoBehaviour
{
    [Header("Core Stats")]
    [Tooltip("Total starting health")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Movement speed; scaled by healthDifficultyMultiplier as health drops")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Multiplier to increase speed (or other stats) as health drops")]
    [SerializeField] private float healthDifficultyMultiplier = 1f;
    private float health;

    [Header("Attack Unlock Thresholds (as % of maxHealth)")]
    [Range(0f, 1f)][SerializeField] private float attack2UnlockThreshold = 0.75f;
    [Range(0f, 1f)][SerializeField] private float attack3UnlockThreshold = 0.50f;

    [Header("Weighted Attack Probabilities")]
    [SerializeField] private float attack1Weight = 1f;
    [SerializeField] private float attack2Weight = 1f;
    [SerializeField] private float attack3Weight = 1f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Crystal Summon Setup")]
    [Tooltip("Empty GameObjects above arena where crystals will spawn")]
    [SerializeField] private Transform[] crystalSpawnPoints;
    [Tooltip("Prefab for normal crystal")]
    [SerializeField] private GameObject normalCrystalPrefab;
    [Tooltip("Prefab for glowing crystal (used below 50% health)")]
    [SerializeField] private GameObject glowingCrystalPrefab;

    // internal state flags
    private bool isIdle = false;
    private bool isHurt = false;

    void Start()
    {
        health = maxHealth;
        // kick off the activation animation
        animator.SetTrigger("Activate");
        // at the end of your "Activate" animation clip, add an Animation Event:
        //   Function Name: OnActivated
    }

    void Update()
    {
        // Only flip sprite when in idle or walk (i.e. isIdle && not hurting)
        if (isIdle && !isHurt)
            FlipTowardsPlayer();
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        spriteRenderer.flipX = (dx < 0);
    }

    // ---- Animation Event Callbacks ----

    // (1) Called via Event at the end of "Activate" animation clip
    public void OnActivated()
    {
        isIdle = true;
        animator.SetTrigger("Idle");
        // At the end of your "Idle" animation loop, add an Animation Event:
        //   Function Name: OnIdleReady
    }

    // (2) Called via Event at the end of each Idle loop
    public void OnIdleReady()
    {
        if (isHurt) return;
        StartCoroutine(PerformNextAttack());
    }

    // (3) Called via Event at the very peak of the crystal?summon animation
    //     (so that crystals actually spawn in sync with the VFX)
    public void SummonCrystals()
    {
        float hpPerc = health / maxHealth;
        GameObject prefabToSpawn = (hpPerc <= attack3UnlockThreshold)
            ? glowingCrystalPrefab
            : normalCrystalPrefab;

        foreach (var pt in crystalSpawnPoints)
            Instantiate(prefabToSpawn, pt.position, pt.rotation);
    }

    // (4) Called via Event at the end of any Attack1, Attack2 or Attack3 clip
    public void OnAttackComplete()
    {
        isIdle = true;
        animator.SetTrigger("Idle");
    }

    // (5) Called via Event at end of Hurt animation
    public void OnHurtComplete()
    {
        isHurt = false;
        isIdle = true;
        animator.SetTrigger("Idle");
    }

    // ---- Public API for Taking Damage ----

    /// <summary>
    /// Call this from your damage system whenever the boss is hit.
    /// </summary>
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        health = Mathf.Clamp(health, 0, maxHealth);

        // optionally scale stats based on remaining health
        float healthFactor = 1f + (1f - health / maxHealth) * healthDifficultyMultiplier;
        animator.speed = healthFactor;  // speed up animations as health drops

        if (isIdle)
        {
            isIdle = false;
            isHurt = true;
            animator.SetTrigger("Hurt");
        }
    }

    // ---- Attack Selection and Execution ----

    private IEnumerator PerformNextAttack()
    {
        isIdle = false;

        // build weighted list of available attacks
        float hpPerc = health / maxHealth;
        var options = new List<(int id, float weight)> { (1, attack1Weight) };
        if (hpPerc <= attack2UnlockThreshold) options.Add((2, attack2Weight));
        if (hpPerc <= attack3UnlockThreshold) options.Add((3, attack3Weight));

        // pick one
        float total = 0;
        foreach (var o in options) total += o.weight;
        float rnd = Random.value * total;
        float cum = 0;
        int chosen = 1;
        foreach (var o in options)
        {
            cum += o.weight;
            if (rnd <= cum) { chosen = o.id; break; }
        }

        // trigger the attack animation:
        animator.SetTrigger("Attack" + chosen);

        // wait until the Animator transitions back to Idle
        // (this requires that each AttackN state has a transition on its exit 
        // via the "OnAttackComplete" event, which calls back into this script)
        yield break; // rest is handled by OnAttackComplete event
    }
}