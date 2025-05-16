using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MageSkeletonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator DamageAnimator;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float shortWalkDistance = 2.0f;
    [SerializeField] private float preferredDistanceToPlayer = 5.0f;

    [Header("Combat")]
    public int maxHealth { get; private set; } = 100;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Probabilities & Timings")]
    [Range(0f, 1f)]
    [SerializeField] private float chanceToWalkRandomly = 0.25f;
    [Range(0f, 1f)]
    [SerializeField] private float chanceToAttackAfterWalk = 0.75f;
    [SerializeField] private float initialIdleDelay = 1.5f;
    [SerializeField] private float timeBetweenActions = 1.0f;

    [Header("Attack1 Burst Settings")]
    [SerializeField] private GameObject attack1ProjectilePrefab;
    [SerializeField] private Transform attack1ProjectileSpawnPoint;
    [SerializeField] private int attack1BurstCount = 4;
    [SerializeField] private float attack1TimeBetweenShotsInBurst = 0.2f;
    [SerializeField] private float attack1ProjectileSpeedForBurst = 10f;

    [Header("Attack2 Radial Burst Settings")]
    [SerializeField] private GameObject dustEffectPrefab; // Pölyefekti
    [SerializeField] private Transform dustAndAttack2ProjectileSpawnPoint; // Yhteinen spawnauspiste pölylle ja Attack2-ammuksille
    [SerializeField] private GameObject attack2ProjectilePrefab; // Projektiili Attack2:lle
    [SerializeField] private int attack2ProjectileCount = 6; // Ammusten määrä
    [SerializeField] private float attack2ProjectileSpeed = 8f; // Attack2 ammusten nopeus


    public int currentHealth { get; private set; }

    private enum State { IdleDecision, Walking, Attacking, Hurt, Dead }
    private State currentState;
    private bool isFacingRight = true;
    private Coroutine currentActionCoroutine;
    private float lastAttackTime = -Mathf.Infinity;

    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashWalk = Animator.StringToHash("Walk");
    private readonly int hashAttack1Prep = Animator.StringToHash("Attack1_prep");
    private readonly int hashAttack2 = Animator.StringToHash("Attack2");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDeath = Animator.StringToHash("Death");

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        FindPlayer();
        if (transform.localScale.x < 0 && isFacingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (transform.localScale.x > 0 && !isFacingRight)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = State.IdleDecision;
        if (playerTransform != null) StartCoroutine(InitialDelay());
        else enabled = false;
    }

    void Update() { /* Kääntyminen hoidetaan liikkumisen yhteydessä */ }

    // --- ANIMATION EVENT FUNKTIOT ---
    public void AE_TriggerAttack2Effects() // Nimi muutettu, kutsutaan Attack2-animaation Eventistä
    {
        // Spawnaa pölyefekti
        if (dustEffectPrefab != null && dustAndAttack2ProjectileSpawnPoint != null)
        {
            Instantiate(dustEffectPrefab, dustAndAttack2ProjectileSpawnPoint.position, dustAndAttack2ProjectileSpawnPoint.rotation);
            //Debug.Log("Dust effect spawned for Attack2.");
        }
        //else Debug.LogWarning("MageSkeletonController: Dust effect prefab or spawn point not set for Attack2!");

        // Ammu Attack2-projektiilit
        FireAttack2ProjectilesInCircle();
    }

    public void AE_StartAttack1Burst() // Kutsutaan Attack1Stage2-animaation Eventistä
    {
        if (attack1ProjectilePrefab == null) { Debug.LogWarning("MageSkeletonController: attack1ProjectilePrefab not set!"); return; }
        if (playerTransform == null) { Debug.LogWarning("MageSkeletonController: PlayerTransform is null, cannot fire Attack1 burst."); return; }
        StartCoroutine(FireAttack1BurstRoutine());
    }
    // --- LOPPU ANIMATION EVENT FUNKTIOT ---


    private IEnumerator FireAttack1BurstRoutine()
    {
        Transform spawnPoint = attack1ProjectileSpawnPoint != null ? attack1ProjectileSpawnPoint : transform;
        for (int i = 0; i < attack1BurstCount; i++)
        {
            if (playerTransform == null || currentState == State.Dead || currentState == State.Hurt) yield break;
            FlipTowardsGivenTarget(playerTransform.position);
            GameObject projectileGO = Instantiate(attack1ProjectilePrefab, spawnPoint.position, spawnPoint.rotation);
            SetupProjectile(projectileGO, playerTransform.position, attack1ProjectileSpeedForBurst);
            if (i < attack1BurstCount - 1) yield return new WaitForSeconds(attack1TimeBetweenShotsInBurst);
        }
    }

    private void FireAttack2ProjectilesInCircle()
    {
        if (attack2ProjectilePrefab == null) { Debug.LogWarning("MageSkeletonController: attack2ProjectilePrefab not set!"); return; }

        Transform spawnPoint = dustAndAttack2ProjectileSpawnPoint != null ? dustAndAttack2ProjectileSpawnPoint : transform;
        float angleStep = 360f / attack2ProjectileCount;
        float currentAngle = 0f; // Aloituskulma, voit satunnaistaa tätä hieman (Random.Range(0f, angleStep))

        //Debug.Log($"Firing {attack2ProjectileCount} projectiles for Attack2 in a circle.");

        for (int i = 0; i < attack2ProjectileCount; i++)
        {
            // Muunna kulma radiaaneiksi ja laske suunta
            float projectileDirX = Mathf.Sin((currentAngle * Mathf.PI) / 180f);
            float projectileDirY = Mathf.Cos((currentAngle * Mathf.PI) / 180f);
            // Yllä oleva laskee siten että 0 astetta on ylöspäin.
            // Jos haluat 0 asteen olevan oikealle:
            // float projectileDirX = Mathf.Cos((currentAngle * Mathf.PI) / 180f);
            // float projectileDirY = Mathf.Sin((currentAngle * Mathf.PI) / 180f);


            Vector2 projectileVector = new Vector2(projectileDirX, projectileDirY);
            Vector3 projectileMoveDirection = projectileVector.normalized;

            // Luo projektiili. Rotaatio asetetaan SetupProjectile-metodissa suunnan mukaan.
            GameObject projectileGO = Instantiate(attack2ProjectilePrefab, spawnPoint.position, Quaternion.identity);

            // Käytä SetupProjectile-metodia, mutta anna sille laskettu suuntavektori targetPositionin sijaan.
            // Tehdään pieni muutos SetupProjectile-metodiin, jotta se voi ottaa vastaan suoran suunnan.
            // Tässä tapauksessa emme tarvitse targetPositionia, koska suunta on jo laskettu.
            // Ohitetaan targetPosition antamalla projektiilin oma sijainti + laskettu suunta.
            SetupProjectile(projectileGO, spawnPoint.position + projectileMoveDirection, attack2ProjectileSpeed);


            currentAngle += angleStep;
        }
    }

    // Muokataan SetupProjectile hieman joustavammaksi
    private void SetupProjectile(GameObject projectileInstance, Vector3 targetOrDirectionPoint, float speed)
    {
        // Jos targetOrDirectionPoint on kaukana, oletetaan sen olevan kohde.
        // Jos se on lähellä (kuten spawnPoint + suuntavektori), oletetaan sen olevan jo suunnattu.
        Vector2 directionToTarget;
        if (Vector3.Distance(projectileInstance.transform.position, targetOrDirectionPoint) > 1.1f) // Pieni kynnysarvo
        {
            directionToTarget = (targetOrDirectionPoint - projectileInstance.transform.position).normalized;
        }
        else
        { // Oletetaan että targetOrDirectionPoint on spawnPoint + normalisoitu suuntavektori
            directionToTarget = (targetOrDirectionPoint - projectileInstance.transform.position).normalized; // Tai suoraan targetOrDirectionPoint, jos se on jo suunta
                                                                                                             // Tässä tapauksessa, koska annoimme spawnPoint.position + projectileMoveDirection, tämä toimii:
                                                                                                             // directionToTarget = (targetOrDirectionPoint - projectileInstance.transform.position).normalized;
                                                                                                             // Yksinkertaisempi voisi olla vain:
                                                                                                             // Vector3 worldDirection = targetOrDirectionPoint - projectileInstance.transform.position;
                                                                                                             // directionToTarget = new Vector2(worldDirection.x, worldDirection.y).normalized;
        }


        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        projectileInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        ProjectileController projectileScript = projectileInstance.GetComponent<ProjectileController>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(directionToTarget, speed);
        }
        else
        {
            Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = directionToTarget * speed;
            else Debug.LogWarning("Projectile needs a Rigidbody2D or a ProjectileController script with Initialize method.", projectileInstance);
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTransform = playerObject.transform;
            else Debug.LogError("MageSkeletonController: Player not found!", this.gameObject);
        }
    }

    private IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(initialIdleDelay);
        if (playerTransform != null) StartNewActionCycle();
    }

    void StartNewActionCycle()
    {
        if (currentState == State.Dead) return;
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        currentState = State.IdleDecision;
        if (!IsAnimatorInState(hashIdle))
        {
            animator.ResetTrigger("Walk"); animator.ResetTrigger("Attack1Prep"); animator.ResetTrigger("Attack2");
            animator.SetTrigger("Idle");
        }
        currentActionCoroutine = StartCoroutine(MainBehaviorLoop());
    }

    private IEnumerator MainBehaviorLoop()
    {
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) && currentState == State.IdleDecision);
        yield return new WaitForSeconds(timeBetweenActions);

        if (currentState != State.IdleDecision) yield break;
        if (playerTransform == null) yield break;

        Vector2 moveDirection;
        if (Random.value <= chanceToWalkRandomly)
        {
            moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            if (moveDirection == Vector2.zero) moveDirection = Vector2.right;
        }
        else
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < preferredDistanceToPlayer) moveDirection = (transform.position - playerTransform.position).normalized;
            else moveDirection = (playerTransform.position - transform.position).normalized;
        }

        FlipBasedOnMoveDirection(moveDirection);
        currentState = State.Walking;
        animator.SetTrigger("Walk");
        yield return StartCoroutine(WalkRoutine(moveDirection, shortWalkDistance));
    }

    private IEnumerator WalkRoutine(Vector2 direction, float distanceToWalk)
    {
        Vector2 startPosition = transform.position;
        float distanceWalked = 0f;
        while (distanceWalked < distanceToWalk && currentState == State.Walking)
        {
            if (playerTransform == null) { yield break; }
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, moveSpeed * Time.deltaTime);
            distanceWalked = Vector2.Distance(startPosition, transform.position);
            yield return null;
        }

        if (currentState == State.Walking)
        {
            animator.ResetTrigger("Walk"); animator.SetTrigger("Idle");
            yield return new WaitUntil(() => IsAnimatorInState(hashIdle));
            currentActionCoroutine = StartCoroutine(MonitorPostWalkActions());
        }
    }

    private IEnumerator MonitorPostWalkActions()
    {
        if (currentState != State.Walking && currentState != State.IdleDecision)
        {
            if (currentState == State.Walking) currentState = State.IdleDecision;
            else yield break;
        }
        currentState = State.IdleDecision;

        bool canAttackNow = Time.time >= lastAttackTime + attackCooldown;
        if (Random.value <= chanceToAttackAfterWalk && canAttackNow)
        {
            currentState = State.Attacking;
            int attackChoice = Random.Range(0, 2);
            if (playerTransform != null) FlipTowardsGivenTarget(playerTransform.position); // Tähtää pelaajaa aina ennen hyökkäystä

            if (attackChoice == 0) animator.SetTrigger("Attack1Prep"); // AE_StartAttack1Burst kutsutaan Attack1Stage2:sta
            else animator.SetTrigger("Attack2"); // AE_TriggerAttack2Effects kutsutaan Attack2:sta
            lastAttackTime = Time.time;
            yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Hurt || currentState == State.Dead);
            StartNewActionCycle();
        }
        else
        {
            StartNewActionCycle();
        }
    }

    private void FlipBasedOnMoveDirection(Vector2 moveDirection)
    {
        if (moveDirection.x > 0.01f && !isFacingRight)
        {
            isFacingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveDirection.x < -0.01f && isFacingRight)
        {
            isFacingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void FlipTowardsGivenTarget(Vector3 targetPosition)
    {
        if (currentState == State.Dead) return;
        Vector2 directionToTarget = targetPosition - transform.position;
        if (directionToTarget.x > 0.01f && !isFacingRight)
        {
            isFacingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionToTarget.x < -0.01f && isFacingRight)
        {
            isFacingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;
        currentHealth -= damage;
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        if (currentHealth <= 0) { Die(); }
        else
        {
            currentState = State.Hurt;
            animator.SetTrigger("Hurt");
            if (DamageAnimator != null) { DamageAnimator.SetTrigger("TakeDamage"); }
            currentActionCoroutine = StartCoroutine(RecoverFromHurt());
        }
        if (CameraShake.instance != null)
        {
            float shakeDuration = 0.1f;
            float shakeMagnitude = 0.5f;
            CameraShake.instance.StartShake(shakeDuration, shakeMagnitude);
        }
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Dead);
        if (currentState != State.Dead) StartNewActionCycle();
    }

    private void Die()
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        StopAllCoroutines();
        currentState = State.Dead;
        animator.ResetTrigger("Walk"); animator.ResetTrigger("Attack1Prep"); animator.ResetTrigger("Attack2"); animator.ResetTrigger("Hurt");
        animator.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) { GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; GetComponent<Rigidbody2D>().simulated = false; }
        Destroy(gameObject, 3f);
    }

    private bool IsAnimatorInState(int stateHash)
    {
        if (animator == null || !animator.isInitialized || !animator.gameObject.activeInHierarchy) return false;
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
    }
}