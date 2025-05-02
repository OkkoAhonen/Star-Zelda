using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{

    public string EnemyID = "9";

    [Header("Configuration")]
    public Enemy enemyStats;
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private Item dropItem;
    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Attacking")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private float deathAnimationDuration = 1.5f; // K�ytet��n lootin ajoitukseen

    // --- LIS�TTY: Knockback-kesto ---
    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.2f; // Kuinka kauan knockback est�� normaalin liikkeen

    [Header("Runtime References")]
    private Transform playerTransform;
    private Rigidbody2D rb;
    private playerAction playerActionScript;

    [Header("State")]
    private float currentHealth;
    private bool isPlayerInDetectionRange = false;
    private bool isMoving = false;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isDead = false;
    private bool isKnockedBack = false; // Knockback-tila
    private Coroutine knockbackCoroutine = null; // Viittaus aktiiviseen knockback-coroutineen
    private bool isFacingRight = true; // Katsomissuunta

    void Start()
    {
        // ---- Alustukset ----
        // POISTETTU TURHA TARKISTUS T�ST�

        if (enemyStats == null) { Debug.LogError($"EnemyStats puuttuu: {gameObject.name}", this); enabled = false; return; }

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning($"Animator component ei l�ytynyt: {gameObject.name}", this);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerActionScript = playerObject.GetComponent<playerAction>();
            if (playerActionScript == null) Debug.LogWarning($"PlayerAction-skripti� ei l�ytynyt pelaajasta.", this);
        }
        else { Debug.LogError("Pelaajaa (tagi 'Player') ei l�ytynyt.", this); enabled = false; return; }

        currentHealth = enemyStats.maxHealth;
        lastAttackTime = -attackCooldown;
        isDead = false;
        if (currentHealth <= 0) { Debug.LogError($"ERROR: Enemy {gameObject.name} started with 0 or less health!", this); isDead = true; }

        if ((isFacingRight && transform.localScale.x < 0) || (!isFacingRight && transform.localScale.x > 0))
        {
            Flip();
        }
    }

    void Update()
    {
        if (isDead) return;
        WatchHealth();
        if (isDead) return;
        CheckPlayerDistance();
        // --- MUOKATTU: HandleActions kutsutaan vain jos ei knockback ---
        // (Tai HandleActions hoitaa itse tarkistuksen alussa)
        HandleActions(); // HandleActions sis�lt�� nyt tarkistuksen
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // --- MUOKATTU EHTO: �l� liiku jos knockback on p��ll� ---
        if (isMoving && !isAttacking && !isKnockedBack)
        {
            MoveTowardsPlayerOneAxis();
        }
        // Nollaa velocity vain jos EI liikuta EIK� olla knockback-tilassa
        else if (!isMoving && !isKnockedBack && rb.linearVelocity != Vector2.zero)
        {
            if (Mathf.Abs(rb.linearVelocity.x) < 0.01f && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null) { isPlayerInDetectionRange = false; return; }
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInDetectionRange = distanceToPlayer <= enemyStats.detectionRange;
    }

    // --- MUOKATTU HandleActions ---
    void HandleActions()
    {
        // --- LIS�TTY TARKISTUS: �l� tee toimintoja knockbackin aikana ---
        if (isKnockedBack)
        {
            isMoving = false; // Varmista, ettei yrit� liikkua
            if (animator != null) animator.SetBool("IsMoving", false); // Varmista animaatio
            return; // Poistu metodista, jos knockback p��ll�
        }
        // --- ---

        bool shouldTryToMove = false;
        if (isPlayerInDetectionRange && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            // --- LIS�TTY TARKISTUS: �l� hy�kk�� knockbackin aikana ---
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking && !isKnockedBack)
            {
                StartAttack();
            }
            // --- LIS�TTY TARKISTUS: �l� ala liikkua knockbackin aikana ---
            else if (!isAttacking && !isKnockedBack) // Huom: !isKnockedBack lis�tty
            {
                shouldTryToMove = true;
            }
        }
        else if (!isAttacking) { shouldTryToMove = false; } // Ei tarvitse isKnockedBack-tarkistusta, koska se tehd��n jo alussa

        isMoving = shouldTryToMove;

        // Lis�tty !isKnockedBack ehtoon
        if (animator != null) { animator.SetBool("IsMoving", isMoving && !isAttacking && !isKnockedBack); }

        // Velocityn nollaus on FixedUpdatessa / Dragilla
        // if (!shouldTryToMove && !isAttacking) { rb.velocity = Vector2.zero; } // T�m� rivi voi olla tarpeeton
    }


    // --- MUOKATTU StartAttack ---
    void StartAttack()
    {
        // --- LIS�TTY TARKISTUS: �l� hy�kk�� knockbackin tai kuoleman aikana ---
        if (isDead || isKnockedBack) return;

        isAttacking = true; isMoving = false; lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero; // Pys�yt� mahdollinen liike

        // K��ntyminen pelaajaa kohti
        if (playerTransform != null) { /* ... k��ntymislogiikka ... */ }

        // Animaattorin p�ivitys
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            if (Random.Range(0, 2) == 0) { animator.SetTrigger("Attack1"); } else { animator.SetTrigger("Attack2"); }
            Debug.Log($"{gameObject.name} triggering Attack");
            DoAttackDamage();
        }
        else { Invoke(nameof(DoAttackDamage), 0.2f); Invoke(nameof(AttackAnimationFinished), 0.5f); }
    }

    public void DoAttackDamage()
    {
        if (isDead  || playerTransform == null) return;
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange * 1.2f)
        {
            Debug.Log($"Enemy ({gameObject.name}) dealing damage via Animation Event!");




            PlayerStatsManager.instance.TakeDamage((int)enemyStats.strength);   
        }
        // else { Debug.Log($"Player moved out of range during {gameObject.name}'s attack animation."); }
    }

    public void AttackAnimationFinished() { isAttacking = false; }

    void MoveTowardsPlayerOneAxis()
    {
        if (playerTransform == null) return;

        Vector2 difference = (Vector2)playerTransform.position - rb.position;
        float xDiff = Mathf.Abs(difference.x);
        float yDiff = Mathf.Abs(difference.y);
        Vector2 moveDirection = Vector2.zero;

        if (xDiff > yDiff) { moveDirection = new Vector2(Mathf.Sign(difference.x), 0); }
        else if (yDiff > 0.01f) { moveDirection = new Vector2(0, Mathf.Sign(difference.y)); }

        if (moveDirection.x > 0 && !isFacingRight) { Flip(); }
        else if (moveDirection.x < 0 && isFacingRight) { Flip(); }

        if (moveDirection != Vector2.zero)
        {
            Vector2 targetPosition = rb.position + moveDirection * enemyStats.speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        else { rb.linearVelocity = Vector2.zero; }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private IEnumerator KnockbackCooldown()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (this != null && gameObject.activeInHierarchy) { isKnockedBack = false; }
        knockbackCoroutine = null;
    }

    void OnCollisionEnter2D(Collision2D collision) { /*... Sama ...*/ }

    // TakeDamage on jo p�ivitetty oikein edellisess� vaiheessa
    public void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce)
    {
        if (isDead) return;
        if (animator != null) { animator.SetTrigger("Hurt"); }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
            isKnockedBack = false;
        }

        rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        // Debug.Log($"{gameObject.name} received knockback force. Velocity after AddForce: {rb.velocity}");

        isKnockedBack = true;
        knockbackCoroutine = StartCoroutine(KnockbackCooldown());

        currentHealth -= damage;
        // Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{enemyStats.maxHealth}");

        if (CameraShake.instance != null)
        {
            float shakeDuration = 0.1f;
            float shakeMagnitude = 0.05f;
            CameraShake.instance.StartShake(shakeDuration, shakeMagnitude);
        }
        else { Debug.LogWarning("CameraShake.instance ei l�ytynyt!"); }
    }


    void WatchHealth()
    {
        if (!isDead && currentHealth <= 0) { Die(); }
    }

    void Die()
    {
        GameEventsManager.instance.playerEvents.GainExperience(50);


        if (isDead) return; isDead = true;
        Debug.Log($"{gameObject.name} is dying and will remain on screen.");
        QuestManager.instance.NotifyStepEvent("Kill", EnemyID);
        isMoving = false; isAttacking = false; rb.linearVelocity = Vector2.zero;
        rb.simulated = false; GetComponent<Collider2D>().enabled = false;
        if (animator != null) { animator.SetTrigger("Die"); StartCoroutine(DropLootAfterDelay(deathAnimationDuration)); }
        else { HandleLootDrop(); }
    }

    IEnumerator DropLootAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); HandleLootDrop();
    }

    void HandleLootDrop()
    {
        if (lootPrefab != null && dropItem != null)
        {
            Debug.Log($"Spawning loot ({dropItem.name}) from {gameObject.name}");
            GameObject lootObject = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            Loot loot = lootObject.GetComponent<Loot>();
            if (loot != null) { loot.Initialize(dropItem); }
            else { Debug.LogWarning($"Loot Prefabilla ({lootPrefab.name}) ei ole Loot-skripti�.", this); }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (enemyStats != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, enemyStats.detectionRange); }
        else { Gizmos.color = Color.gray; Gizmos.DrawWireSphere(transform.position, 5f); }
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}