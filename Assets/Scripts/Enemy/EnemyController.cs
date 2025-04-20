using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    public Enemy enemyStats;
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private Item dropItem;
    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Attacking")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private float deathAnimationDuration = 1.5f; // Käytetään lootin ajoitukseen

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
    // --- LISÄTTY: Katsomissuunta ---
    private bool isFacingRight = true; // Oletetaan, että sprite katsoo oletuksena oikealle

    void Start()
    {
        // ---- Alustukset ----
        if (enemyStats == null) { Debug.LogError($"EnemyStats puuttuu: {gameObject.name}", this); enabled = false; return; }

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning($"Animator component ei löytynyt: {gameObject.name}", this);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerActionScript = playerObject.GetComponent<playerAction>();
            if (playerActionScript == null) Debug.LogWarning($"PlayerAction-skriptiä ei löytynyt pelaajasta.", this);
        }
        else { Debug.LogError("Pelaajaa (tagi 'Player') ei löytynyt.", this); enabled = false; return; }

        currentHealth = enemyStats.maxHealth;
        lastAttackTime = -attackCooldown;
        isDead = false;
        if (currentHealth <= 0) { Debug.LogError($"ERROR: Enemy {gameObject.name} started with 0 or less health!", this); isDead = true; }

        // --- LISÄTTY: Varmista alkuasento ---
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
        HandleActions();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isMoving && !isAttacking)
        {
            MoveTowardsPlayerOneAxis();
        }
        else if (!isMoving && rb.velocity != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null) { isPlayerInDetectionRange = false; return; }
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInDetectionRange = distanceToPlayer <= enemyStats.detectionRange;
    }

    void HandleActions()
    {
        bool shouldTryToMove = false;
        if (isPlayerInDetectionRange && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking) { StartAttack(); }
            else if (!isAttacking) { shouldTryToMove = true; }
        }
        else if (!isAttacking) { shouldTryToMove = false; }

        isMoving = shouldTryToMove;

        if (animator != null) { animator.SetBool("IsMoving", isMoving && !isAttacking); }

        if (!shouldTryToMove && !isAttacking) { rb.velocity = Vector2.zero; }
    }

    void StartAttack()
    {
        if (isDead) return;
        isAttacking = true; isMoving = false; lastAttackTime = Time.time;
        rb.velocity = Vector2.zero;

        // --- LISÄTTY: Käänny pelaajaa kohti ennen hyökkäystä (suositeltavaa, mutta voidaan jättää pois pyynnöstä) ---
        if (playerTransform != null)
        {
            float directionToPlayerX = playerTransform.position.x - transform.position.x;
            if (directionToPlayerX > 0.01f && !isFacingRight) // Pelaaja oikealla, katson vasemmalle
            {
                Flip();
            }
            else if (directionToPlayerX < -0.01f && isFacingRight) // Pelaaja vasemmalla, katson oikealle
            {
                Flip();
            }
        }
        // --- Kääntymislogiikka hyökkäyksessä päättyy ---


        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            if (Random.Range(0, 2) == 0) { animator.SetTrigger("Attack1"); } else { animator.SetTrigger("Attack2"); }
            Debug.Log($"{gameObject.name} triggering Attack");
        }
        else { Invoke(nameof(DoAttackDamage), 0.2f); Invoke(nameof(AttackAnimationFinished), 0.5f); }
    }

    public void DoAttackDamage()
    {
        if (isDead || playerActionScript == null || playerTransform == null) return;
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange * 1.2f)
        {
            Debug.Log($"Enemy ({gameObject.name}) dealing damage via Animation Event!");
            playerActionScript.playerTakeDamage(enemyStats.strength);
        }
        else { Debug.Log($"Player moved out of range during {gameObject.name}'s attack animation."); }
    }

    public void AttackAnimationFinished() { isAttacking = false; }


    // --- MUOKATTU LIIKKUMISLOGIIKKA ---
    void MoveTowardsPlayerOneAxis()
    {
        if (playerTransform == null) return; // Varmistus

        Vector2 difference = (Vector2)playerTransform.position - rb.position;
        float xDiff = Mathf.Abs(difference.x);
        float yDiff = Mathf.Abs(difference.y);
        Vector2 moveDirection = Vector2.zero;

        if (xDiff > yDiff)
        {
            moveDirection = new Vector2(Mathf.Sign(difference.x), 0);
        }
        else if (yDiff > 0.01f) // Pieni toleranssi
        {
            moveDirection = new Vector2(0, Mathf.Sign(difference.y));
        }

        // --- LISÄTTY: Kääntyminen liikkeen X-suunnan mukaan ---
        if (moveDirection.x > 0 && !isFacingRight) // Liikkuu oikealle, mutta katsoo vasemmalle
        {
            Flip();
        }
        else if (moveDirection.x < 0 && isFacingRight) // Liikkuu vasemmalle, mutta katsoo oikealle
        {
            Flip();
        }
        // --- Kääntymislogiikka päättyy ---

        if (moveDirection != Vector2.zero)
        {
            Vector2 targetPosition = rb.position + moveDirection * enemyStats.speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        else { rb.velocity = Vector2.zero; }
    }
    // --- LIIKKUMISLOGIIKAN MUOKKAUS PÄÄTTYY ---

    // --- LISÄTTY: Metodi flippausta varten ---
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    // --- ---

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player")) { /* Kontaktivahinko */ }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{enemyStats.maxHealth}");
        // if (animator != null) animator.SetTrigger("Hit");
    }

    void WatchHealth()
    {
        if (!isDead && currentHealth <= 0) { Die(); }
    }

    void Die()
    {
        if (isDead) return; isDead = true;
        Debug.Log($"{gameObject.name} is dying and will remain on screen.");
        isMoving = false; isAttacking = false; rb.velocity = Vector2.zero;
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
            else { Debug.LogWarning($"Loot Prefabilla ({lootPrefab.name}) ei ole Loot-skriptiä.", this); }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (enemyStats != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, enemyStats.detectionRange); }
        else { Gizmos.color = Color.gray; Gizmos.DrawWireSphere(transform.position, 5f); }
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}