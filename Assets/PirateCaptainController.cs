using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PirateCaptainController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator DamageAnimator; // Jos käytössä erilliselle vahinkoefektianimaattorille

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f; // Normaali kävelynopeus
    [SerializeField] private float shortWalkDistance = 2.5f; // Matka, jonka kävelee normaalisti kohti pelaajaa
    [SerializeField] private float approachXSpeed = 4.0f; // Nopeus, jolla lähestyy pelaajan X-koordinaattia Attack1:tä varten
    [SerializeField] private float spinMoveSpeed = 5.0f; // Nopeus Attack2-spinnauksen aikana

    [Header("Combat - General")]
    public int maxHealth = 120;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float closeRangeAttackThreshold = 3.0f; // Etäisyys, jonka sisällä Attack2 (spin) valitaan

    [Header("Attack1 (Pistol Shot)")]
    [SerializeField] private GameObject pistolProjectilePrefab;
    [SerializeField] private Transform pistolProjectileSpawnPoint;
    [SerializeField] private float pistolProjectileSpeed = 12f;

    [Header("Attack2 (Spin)")]
    [SerializeField] private float spinDuration = 2.0f; // Kuinka kauan spinnaus kestää
    // Attack2 ei välttämättä ammu projektiileja, jos se on vain spinnausliike.
    // Jos haluat sen tekevän vahinkoa osuessaan, se pitää toteuttaa esim. Colliderilla.

    [Header("Timings & Probabilities")]
    [SerializeField] private float initialIdleDelay = 1.0f;
    [SerializeField] private float idleDurationAfterCycle = 1.5f; // Aika idlessä ennen uutta kävelyä
    [Range(0f, 1f)]
    [SerializeField] private float chanceToAttackAfterWalk = 0.75f; // 75% -> Kohtaan 4, 25% -> Kohtaan 1

    public int currentHealth;
    private enum State { Idling, WalkingToPlayer, DecidingAfterWalk, ApproachingForAttack1, PerformingAttack1, PreparingAttack2, SpinningAttack2, EndingAttack2, Hurt, Dead }
    private State currentState;
    private bool isFacingRight = true;
    private Coroutine currentActionCoroutine;
    private float lastAttackTime = -Mathf.Infinity;

    // Päivitä nämä vastaamaan PIRAATIN animaatioiden nimiä!
    private readonly int hashIdle = Animator.StringToHash("female pirate-idle");
    private readonly int hashWalk = Animator.StringToHash("female pirate-walk");
    private readonly int hashAttack1 = Animator.StringToHash("female pirate-attack1");
    private readonly int hashAttack2Prep = Animator.StringToHash("female pirate-attack2 prep");
    private readonly int hashAttack2Loop = Animator.StringToHash("female pirate-attack2 loop");
    private readonly int hashAttack2End = Animator.StringToHash("female pirate-attack2 end"); // Varmista että tämä on Animatorissa
    private readonly int hashHurt = Animator.StringToHash("female pirate-hurt");
    private readonly int hashDeath = Animator.StringToHash("female pirate-death");

    // Trigger-nimet (varmista että vastaavat Animator Controllerin parametreja)
    private const string triggerIdle = "Idle";
    private const string triggerWalk = "Walk";
    private const string triggerAttack1 = "Attack1";
    private const string triggerAttack2Prep = "Attack2Prep";
    private const string triggerEndSpin = "EndSpin"; // Uusi trigger spinnauksen lopettamiseen
    private const string triggerHurt = "Hurt";
    private const string triggerDeath = "Death";


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
        SetState(State.Idling); // Aloitetaan Idling-tilasta
        if (playerTransform != null) StartCoroutine(InitialDelay());
        else enabled = false;
    }

    void Update() { /* Ei tarvita tällä hetkellä, jos kääntyminen on sidottu liikkeeseen */ }

    // --- ANIMATION EVENT FUNKTIOT ---
    public void AE_FirePistolProjectile() // Kutsutaan female pirate-attack1 -animaation Eventistä
    {
        if (pistolProjectilePrefab == null) { Debug.LogWarning("Pirate: pistolProjectilePrefab not set!"); return; }
        if (playerTransform == null) { Debug.LogWarning("Pirate: PlayerTransform is null, cannot fire pistol."); return; }

        Transform spawnPoint = pistolProjectileSpawnPoint != null ? pistolProjectileSpawnPoint : transform;
        GameObject projectileGO = Instantiate(pistolProjectilePrefab, spawnPoint.position, spawnPoint.rotation);
        SetupProjectile(projectileGO, playerTransform.position, pistolProjectileSpeed);
    }
    // Attack2:n spinnausliike ja vahinko hoidetaan korutiinissa, ei välttämättä tarvita eventtiä itse spinnaukselle,
    // ellei haluta jotain erikoisefektiä loopin aikana. Prep ja End hoidetaan siirtymillä.

    private void SetupProjectile(GameObject projectileInstance, Vector3 targetPosition, float speed)
    {
        Vector2 directionToTarget = (targetPosition - projectileInstance.transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        projectileInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        ProjectileController projectileScript = projectileInstance.GetComponent<ProjectileController>();
        if (projectileScript != null) projectileScript.Initialize(directionToTarget, speed);
        else
        {
            Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = directionToTarget * speed;
            else Debug.LogWarning("Projectile needs Rigidbody2D or ProjectileController script.", projectileInstance);
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTransform = playerObject.transform;
            else Debug.LogError("Pirate: Player not found!", this.gameObject);
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

        SetState(State.Idling); // Kohta 1 alkaa tästä

        currentActionCoroutine = StartCoroutine(MainBehaviorLoop());
    }

    private void SetState(State newState)
    {
        currentState = newState;
        // Debug.Log("Pirate new state: " + newState); // Hyvä debuggaukseen

        // Aseta animaattorin tila tarvittaessa (usein triggerit riittävät)
        switch (newState)
        {
            case State.Idling:
                //animator.SetTrigger(triggerIdle);
                break;
            case State.WalkingToPlayer:
            case State.ApproachingForAttack1: // Nämä molemmat käyttävät kävelyanimaatiota
                animator.SetTrigger(triggerWalk);
                break;
                // Muut tilat (Attacking, Spinning, jne.) käynnistävät omat animaationsa triggereillä metodeissaan
        }
    }


    private IEnumerator MainBehaviorLoop()
    {
        // Kohta 1: Idle-vaihe
        // SetState(State.Idling) on jo kutsuttu StartNewActionCycle-metodissa.
        // Odotetaan animaattorin siirtymistä Idle-tilaan varmuuden vuoksi.
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle));
        yield return new WaitForSeconds(idleDurationAfterCycle);

        if (ShouldInterrupt()) yield break;

        // Kohta 2: Kävely kohti pelaajaa
        SetState(State.WalkingToPlayer);
        yield return StartCoroutine(WalkRoutine(playerTransform.position, shortWalkDistance, moveSpeed));

        if (ShouldInterrupt()) yield break;
        SetState(State.DecidingAfterWalk); // Valmistellaan päätöksentekoa varten

        // Kohta 3: Päätös kävelyn jälkeen
        // Odotetaan, että ollaan takaisin Idlessä animaattorissa ennen päätöstä
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle));

        if (Random.value <= chanceToAttackAfterWalk) // 75% -> Kohtaan 4 (Hyökkäys)
        {
            // Kohta 4: Hyökkäyspäätös
            if (playerTransform == null) { StartNewActionCycle(); yield break; } // Pelaaja kadonnut

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            FlipTowardsGivenTarget(playerTransform.position); // Käänny kohti pelaajaa ennen hyökkäyspäätöstä

            if (distanceToPlayer <= closeRangeAttackThreshold && Time.time >= lastAttackTime + attackCooldown)
            {
                // Aloita Attack2 (Spin)
                SetState(State.PreparingAttack2);
                animator.SetTrigger(triggerAttack2Prep);
                lastAttackTime = Time.time;
                // Odota prep-animaation päättymistä ja siirtymistä loop-tilaan
                yield return new WaitUntil(() => IsAnimatorInState(hashAttack2Loop) || ShouldInterrupt());
                if (ShouldInterrupt()) yield break;

                SetState(State.SpinningAttack2);
                yield return StartCoroutine(SpinAttackRoutine()); // Itse spinnausliike

                // Spinnauksen jälkeen (SpinAttackRoutine hoitaa EndSpin-triggerin)
                // Odota end-animaation päättymistä ja paluuta Idleen
                yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || ShouldInterrupt());

            }
            else if (Time.time >= lastAttackTime + attackCooldown) // Jos ei lähellä, mutta voi hyökätä -> Attack1
            {
                // Aloita Attack1 (Pistol)
                SetState(State.ApproachingForAttack1);
                // Liiku pelaajan X-tasolle
                yield return StartCoroutine(ApproachPlayerXRoutine());
                if (ShouldInterrupt()) yield break;

                // Varmista kääntyminen ja ammu
                FlipTowardsGivenTarget(playerTransform.position);
                SetState(State.PerformingAttack1);
                animator.SetTrigger(triggerAttack1); // Animation Event hoitaa ampumisen
                lastAttackTime = Time.time;
                // Odota attack1-animaation päättymistä ja paluuta Idleen
                yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || ShouldInterrupt());
            }
            // Jos kumpikaan hyökkäys ei ollut mahdollinen (esim. cooldown), mennään syklin loppuun
        }
        //else: 25% mahdollisuus, että ei tehdä mitään ja siirrytään suoraan syklin loppuun

        // Kohta 5: Siirry kohtaan 1
        StartNewActionCycle();
    }

    private bool ShouldInterrupt() // Tarkistaa, pitäisikö nykyinen toiminto keskeyttää
    {
        return currentState == State.Hurt || currentState == State.Dead || playerTransform == null;
    }

    private IEnumerator WalkRoutine(Vector3 targetPosition, float distanceOrUntilTarget, float speed, bool stopAtTarget = false)
    {
        FlipBasedOnDirectionToTarget(targetPosition); // Käänny kohti kohdetta ennen kävelyä

        Vector2 startPos = transform.position;
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f && // Liiku kunnes lähellä kohdetta
               Vector2.Distance(startPos, transform.position) < distanceOrUntilTarget && // Tai kunnes tietty matka kuljettu
               currentState != State.Hurt && currentState != State.Dead) // Ja ei olla loukkaantuneita/kuolleita
        {
            if (playerTransform == null) yield break;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Jos stopAtTarget on true ja ollaan lähellä kohdetta, lopeta
            if (stopAtTarget && Vector2.Distance(transform.position, targetPosition) <= 0.1f) break;

            yield return null;
        }
        // Pysäytä kävelyanimaatio ja palaa Idleen (Animator Controllerin kautta)
        Debug.Log("StopWalk");
        animator.SetTrigger(triggerIdle);
    }

    private IEnumerator ApproachPlayerXRoutine()
    {
        if (playerTransform == null) yield break;
        Vector3 targetXPosition = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
        // Käytä WalkRoutinea liikkumaan vain X-suunnassa
        yield return StartCoroutine(WalkRoutine(targetXPosition, Mathf.Infinity, approachXSpeed, true)); // Liiku kunnes X-kohde saavutettu
    }

    private IEnumerator SpinAttackRoutine()
    {
        float spinTimer = 0f;
        while (spinTimer < spinDuration && currentState == State.SpinningAttack2) // Varmista tila
        {
            if (playerTransform == null) { animator.SetTrigger(triggerEndSpin); yield break; }

            // Liiku aggressiivisesti pelaajaa kohti
            FlipTowardsGivenTarget(playerTransform.position); // Pidä suunta pelaajaan
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, spinMoveSpeed * Time.deltaTime);

            // Tässä voisi olla osuman tarkistuslogiikka, jos spinnaus tekee vahinkoa kontaktista
            // Esim. pieni OverlapCircle pelaajan ympärillä.

            spinTimer += Time.deltaTime;
            yield return null;
        }
        // Aseta triggeri lopettamaan spinnausanimaatio, jos se ei ole jo keskeytynyt
        if (currentState == State.SpinningAttack2)
        {
            animator.SetTrigger(triggerEndSpin);
            SetState(State.EndingAttack2); // Päivitä skriptin tila
        }
    }


    private void FlipBasedOnDirectionToTarget(Vector3 targetPosition)
    {
        Vector2 moveDirection = (targetPosition - transform.position);
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

    private void FlipTowardsGivenTarget(Vector3 targetPosition) // Tämä on sama kuin edellinen, voisi yhdistää
    {
        if (currentState == State.Dead) return;
        FlipBasedOnDirectionToTarget(targetPosition);
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;
        currentHealth -= damage;
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        SetState(State.Hurt); // Aseta tila ennen animaatiota
        animator.SetTrigger(triggerHurt);
        if (DamageAnimator != null) { DamageAnimator.SetTrigger("TakeDamage"); } // Oletetaan triggerin nimi

        if (currentHealth <= 0) Die();
        else currentActionCoroutine = StartCoroutine(RecoverFromHurt());

        // CameraShake.instance... (jos käytössä)
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Dead); // Odota paluuta Idleen
        if (currentState != State.Dead) StartNewActionCycle();
    }

    private void Die()
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        StopAllCoroutines(); // Pysäytä kaikki tämän skriptin korutiinit
        SetState(State.Dead);
        // Nollaa kaikki mahdolliset hyökkäys/liike triggerit ennen Death-triggeriä
        animator.ResetTrigger(triggerWalk); animator.ResetTrigger(triggerAttack1);
        animator.ResetTrigger(triggerAttack2Prep); animator.ResetTrigger(triggerEndSpin);
        animator.ResetTrigger(triggerHurt); // Varmuuden vuoksi
        animator.SetTrigger(triggerDeath);

        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) { GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; GetComponent<Rigidbody2D>().simulated = false; }
        Destroy(gameObject, 3f); // Säädä kuolinanimaation kestoa vastaavaksi
    }

    private bool IsAnimatorInState(int stateHash)
    {
        if (animator == null || !animator.isInitialized || !animator.gameObject.activeInHierarchy) return false;
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
    }
}