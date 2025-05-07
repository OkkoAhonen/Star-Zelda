using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MageSkeletonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float shortWalkDistance = 2.0f; // Matka, jonka kävelee kerrallaan
    [SerializeField] private float preferredDistanceToPlayer = 5.0f; // Etäisyys, jota yrittää ylläpitää

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackCooldown = 2.0f;
    // Min/Max attack rangeja ei enää tarvita tässä mallissa, hyökkäyspäätös on todennäköisyyspohjainen kävelyn jälkeen

    [Header("Probabilities & Timings")]
    [Range(0f, 1f)]
    [SerializeField] private float chanceToWalkRandomly = 0.25f; // 25%
    [Range(0f, 1f)]
    [SerializeField] private float chanceToAttackAfterWalk = 0.75f; // 75%
    [SerializeField] private float initialIdleDelay = 1.5f;
    [SerializeField] private float timeBetweenActions = 1.0f; // Tauko ennen uutta liike/hyökkäys-sykliä

    [Header("Effects")]
    [Tooltip("Raahaa tähän PölyPrefab, joka soitetaan Attack2:n aikana.")]
    [SerializeField] private GameObject dustEffectPrefab;
    [Tooltip("Luo tyhjä GameObject MageSkeletonin lapseksi ja sijoita se kohtaan, johon pölyefekti ilmestyy. Raahaa se tähän.")]
    [SerializeField] private Transform dustEffectSpawnPoint;

    private int currentHealth;
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
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = State.IdleDecision;
        if (playerTransform != null)
        {
            StartCoroutine(InitialDelay());
        }
        else
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform != null && currentState != State.Dead && currentState != State.Hurt && currentState != State.Attacking)
        {
            FlipTowardsPlayer();
        }
    }

    // Tämä funktio kutsutaan Animation Eventistä
    public void AE_SpawnAttack2DustEffect()
    {
        if (dustEffectPrefab != null && dustEffectSpawnPoint != null)
        {
            // Luo pölyefekti-instanssi määriteltyyn paikkaan ja rotaatioon
            Instantiate(dustEffectPrefab, dustEffectSpawnPoint.position, dustEffectSpawnPoint.rotation);
            Debug.Log("Dust effect spawned via Animation Event for Attack2.");
        }
        else
        {
            if (dustEffectPrefab == null)
            {
                Debug.LogWarning("MageSkeletonController: dustEffectPrefab is not assigned in the Inspector!");
            }
            if (dustEffectSpawnPoint == null)
            {
                Debug.LogWarning("MageSkeletonController: dustEffectSpawnPoint is not assigned in the Inspector! You can create an empty child object for this.");
            }
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

    // Käynnistää uuden täyden toimintosyklin alusta
    void StartNewActionCycle()
    {
        if (currentState == State.Dead) return;

        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        currentState = State.IdleDecision; // Valmis uuteen päätökseen
        // Varmistetaan Idle-animaatio, jos ei jo ole
        if (!IsAnimatorInState(hashIdle))
        {
            animator.ResetTrigger("Walk"); // Nollaa muut triggerit
            animator.ResetTrigger("Attack1Prep");
            animator.ResetTrigger("Attack2");
            animator.SetTrigger("Idle");
        }
        currentActionCoroutine = StartCoroutine(MainBehaviorLoop());
        Debug.Log("Starting new Action Cycle.");
    }

    private IEnumerator MainBehaviorLoop()
    {
        // 0. Odotetaan, että ollaan varmasti idlessä ja valmiita
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) && currentState == State.IdleDecision);
        Debug.Log("MainBehaviorLoop: Idle confirmed. Waiting for timeBetweenActions.");

        // 1. Tauko ennen toimintoja
        yield return new WaitForSeconds(timeBetweenActions);

        if (currentState != State.IdleDecision) // Tarkistus, jos tila muuttui tauon aikana (esim. Hurt)
        {
            Debug.Log($"MainBehaviorLoop: State changed to {currentState} during timeBetweenActions. Restarting cycle logic if not dead/hurt.");
            // Jos ei ole Hurt tai Dead, StartNewActionCycle kutsutaan RecoverFromHurtista
            // Joten tässä ei välttämättä tarvitse tehdä muuta kuin antaa korutiinin päättyä
            yield break;
        }

        if (playerTransform == null)
        {
            Debug.Log("MainBehaviorLoop: Player is null. Ending current cycle.");
            yield break; // Pelaajaa ei ole, lopetetaan tämä sykli (voisi yrittää löytää uudelleen)
        }

        // 2. Liikkumispäätös
        Vector2 moveDirection;
        float randomValueForMoveType = Random.value; // Arvo 0.0 ja 1.0 välillä

        if (randomValueForMoveType <= chanceToWalkRandomly) // 25% satunnainen liike
        {
            moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            if (moveDirection == Vector2.zero) moveDirection = Vector2.right; // Varmistus ettei ole nollavektori
            Debug.Log($"MainBehaviorLoop: Decided to walk randomly. Direction: {moveDirection}");
        }
        else // 75% pelaajaan reagoiva liike
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < preferredDistanceToPlayer)
            {
                moveDirection = (transform.position - playerTransform.position).normalized; // Poispäin
                Debug.Log($"MainBehaviorLoop: Player too close ({distanceToPlayer} < {preferredDistanceToPlayer}). Moving away.");
            }
            else
            {
                moveDirection = (playerTransform.position - transform.position).normalized; // Kohti
                Debug.Log($"MainBehaviorLoop: Player far enough ({distanceToPlayer} >= {preferredDistanceToPlayer}). Moving towards.");
            }
        }

        // 3. Liikkuminen
        currentState = State.Walking;
        animator.SetTrigger("Walk");
        Debug.Log("MainBehaviorLoop: Starting WalkRoutine.");
        yield return StartCoroutine(WalkRoutine(moveDirection, shortWalkDistance)); // Odota kävelyn päättymistä

        // WalkRoutine kutsuu MonitorPostWalkActions() lopuksi, jos se päättyy normaalisti

    }

    private IEnumerator WalkRoutine(Vector2 direction, float distanceToWalk)
    {
        Debug.Log($"WalkRoutine started. Direction: {direction}, Distance: {distanceToWalk}");
        Vector2 startPosition = transform.position;
        float distanceWalked = 0f;

        while (distanceWalked < distanceToWalk && currentState == State.Walking)
        {
            if (playerTransform == null) { yield break; } // Lopeta jos pelaaja katoaa

            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, moveSpeed * Time.deltaTime);
            distanceWalked = Vector2.Distance(startPosition, transform.position);
            yield return null;
        }

        if (currentState == State.Walking) // Jos kävely päättyi normaalisti (ei keskeytynyt Hurtiin)
        {
            Debug.Log("WalkRoutine finished. Resetting Walk trigger.");
            animator.ResetTrigger("Walk"); // Nollaa triggeri, jotta Idle-siirtymä voi tapahtua
            animator.SetTrigger("Idle");   // Varmista paluu Idleen (Animator Controllerin siirtymä Walk->Idle pitäisi hoitaa tämä)

            // Odotetaan hetki, että animaattori ehtii varmasti Idleen
            yield return new WaitUntil(() => IsAnimatorInState(hashIdle));
            Debug.Log("WalkRoutine: Animator confirmed Idle. Starting MonitorPostWalkActions.");

            // Siirretään päätöksenteko kävelyn jälkeen omaan korutiiniin
            currentActionCoroutine = StartCoroutine(MonitorPostWalkActions());
        }
        else
        {
            Debug.Log($"WalkRoutine interrupted. Current state: {currentState}");
            // Jos Hurt/Dead, niiden logiikka hoitaa jatkon.
        }
    }

    // Korutiini kävelyn jälkeisille päätöksille
    private IEnumerator MonitorPostWalkActions()
    {
        Debug.Log("MonitorPostWalkActions: Deciding action after walk.");
        // Varmistetaan, että ollaan edelleen valmiita päätökseen
        if (currentState != State.Walking && currentState != State.IdleDecision) // Tila on voinut muuttua
        {
            // Jos tila on Walking, se tarkoittaa että WalkRoutine ei asettanut sitä oikein.
            // IdleDecision on ok tila tässä.
            if (currentState == State.Walking) currentState = State.IdleDecision; // Korjataan tila
            else
            {
                Debug.Log($"MonitorPostWalkActions: State is {currentState}, not suitable for post-walk decision. Ending.");
                yield break; // Jokin muu on ottanut kontrollin (Hurt, Dead)
            }
        }
        currentState = State.IdleDecision; // Varmistetaan tila

        float randomValueForAttack = Random.value;
        bool canAttackNow = Time.time >= lastAttackTime + attackCooldown;

        if (randomValueForAttack <= chanceToAttackAfterWalk && canAttackNow) // 75% hyökkäys
        {
            currentState = State.Attacking;
            int attackChoice = Random.Range(0, 2);
            if (attackChoice == 0) animator.SetTrigger("Attack1Prep");
            else animator.SetTrigger("Attack2");
            lastAttackTime = Time.time;
            Debug.Log($"MonitorPostWalkActions: Decided to Attack {(attackChoice == 0 ? "1" : "2")}.");

            // Odota hyökkäyksen valmistumista (Animatorin paluuta Idleen)
            yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Hurt || currentState == State.Dead);

            Debug.Log("MonitorPostWalkActions: Attack finished (or interrupted). Starting new action cycle.");
            StartNewActionCycle(); // Aloita koko sykli alusta hyökkäyksen jälkeen
        }
        else // 25% uusi liike (tai hyökkäys cooldownilla)
        {
            if (!canAttackNow && randomValueForAttack <= chanceToAttackAfterWalk)
            {
                Debug.Log("MonitorPostWalkActions: Wanted to attack, but on cooldown. Starting new action cycle (will move).");
            }
            else
            {
                Debug.Log("MonitorPostWalkActions: Attack chance failed (25%). Starting new action cycle (will move).");
            }
            StartNewActionCycle(); // Aloita koko sykli alusta (johtaa uuteen liikkumiseen)
        }
    }


    private void FlipTowardsPlayer()
    {
        if (playerTransform == null || currentState == State.Dead) return;
        Vector2 directionToPlayer = playerTransform.position - transform.position;
        if (directionToPlayer.x > 0.01f && !isFacingRight)
        {
            isFacingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionToPlayer.x < -0.01f && isFacingRight)
        {
            isFacingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        if (currentHealth <= 0) Die();
        else
        {
            currentState = State.Hurt;
            animator.SetTrigger("Hurt");
            currentActionCoroutine = StartCoroutine(RecoverFromHurt());
        }
    }

    private IEnumerator RecoverFromHurt()
    {
        Debug.Log("RecoverFromHurt: Waiting for Hurt animation to return to Idle...");
        // Odota kunnes animaattori on palannut Idleen TAI vihollinen kuolee
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Dead);

        if (currentState != State.Dead)
        {
            Debug.Log("Recovered from Hurt, starting new action cycle.");
            StartNewActionCycle(); // Aloita koko sykli alusta
        }
        else
        {
            Debug.Log("Died during Hurt recovery.");
        }
    }

    private void Die()
    {
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        StopAllCoroutines();
        Debug.Log("Triggering Death");
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