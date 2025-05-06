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
    // [SerializeField] private float walkDuration = 2.0f; // Ei en�� tarvita n�in
    [SerializeField] private float desiredStoppingDistance = 2.5f; // Kuinka l�helle pelaajaa k�vell��n

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackRange = 3.0f; // Maksimiet�isyys hy�kk�ykselle
    [SerializeField] private float attackCooldown = 2.0f; // Aika hy�kk�ysten v�lill�

    [Header("Timings")]
    [SerializeField] private float initialIdleDelay = 1.5f;
    [SerializeField] private float timeBetweenActions = 1.0f; // Odotus Idless� p��t�sten v�lill�

    private int currentHealth;
    private enum State { IdleDecision, Walking, Attacking, Hurt, Dead }
    private State currentState;
    private bool isFacingRight = true;
    private Coroutine currentActionCoroutine;
    private float lastAttackTime = -Mathf.Infinity; // Alustetaan niin, ett� voi hy�k�t� heti

    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashWalk = Animator.StringToHash("Walk");
    private readonly int hashAttack1Prep = Animator.StringToHash("Attack1_prep");
    private readonly int hashAttack2 = Animator.StringToHash("Attack2");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDeath = Animator.StringToHash("Death");

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        FindPlayer(); // Etsit��n pelaaja
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = State.IdleDecision;
        if (playerTransform != null) // Aloita vain jos pelaaja l�ytyi
        {
            StartCoroutine(InitialDelay());
        }
        else
        {
            enabled = false; // Poista skripti k�yt�st� jos pelaajaa ei ole
        }
    }

    void Update()
    {
        // Jatkuva k��ntyminen pelaajaa kohti voi tehd� vihollisesta el�v�mm�n,
        // mutta vain jos se ei ole hy�kk��m�ss� tai ottamassa vahinkoa.
        if (playerTransform != null && currentState != State.Dead && currentState != State.Hurt && currentState != State.Attacking)
        {
            FlipTowardsPlayer();
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                Debug.Log("Player found.");
            }
            else
            {
                Debug.LogError("MageSkeletonController: Player object with tag 'Player' not found!", this.gameObject);
            }
        }
    }

    private IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(initialIdleDelay);
        if (playerTransform != null) // Tarkistus uudelleen
        {
            StartActionLoop();
        }
    }

    void StartActionLoop()
    {
        // Debug.Log($"StartActionLoop called. Current State: {currentState}, Animator State: {GetCurrentAnimatorStateName()}");

        // Varmista, ettei vanha korutiini j�� py�rim��n, paitsi jos se on jo oikea
        if (currentActionCoroutine != null)
        {
            // Tarkista onko k�ynniss� jo ActionLoop, jos on, �l� tee mit��n
            // T�m� vaatisi korutiinin nimen tallentamista tai monimutkaisempaa hallintaa.
            // Yksinkertaisempi tapa: pys�yt� aina vanha ja aloita uusi.
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null; // Nollaa viittaus
        }

        currentState = State.IdleDecision; // Aseta aina tila oikein
        // Ei v�ltt�m�tt� tarvitse trigger�id� Idle� t�ss�, jos paluu Idleen toimii muuten
        // animator.SetTrigger("Idle");

        currentActionCoroutine = StartCoroutine(ActionLoop());
    }

    private IEnumerator ActionLoop()
    {
        Debug.Log($"ActionLoop started. Waiting for Idle state and IdleDecision.");

        while (currentState != State.Dead)
        {
            // Odotetaan, ett� animaattori on Idless� JA skriptin tila on IdleDecision
            yield return new WaitUntil(() => IsAnimatorInState(hashIdle) && currentState == State.IdleDecision);
            Debug.Log("ActionLoop: Conditions met (Animator Idle, Script IdleDecision). Waiting for timeBetweenActions.");

            // Pieni tauko Idless� ennen seuraavaa p��t�st�
            yield return new WaitForSeconds(timeBetweenActions);

            // Tarkistetaan tila uudelleen tauon j�lkeen (jos otti vahinkoa)
            if (currentState != State.IdleDecision)
            {
                Debug.Log($"ActionLoop: State changed during idle pause ({currentState}). Restarting WaitUntil.");
                continue; // Aloita WaitUntil alusta
            }

            // Laske et�isyys pelaajaan
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            bool canAttack = Time.time >= lastAttackTime + attackCooldown;

            Debug.Log($"ActionLoop: Deciding action. Distance: {distanceToPlayer}, CanAttack: {canAttack}");

            // --- P��t�ksentekologiikka ---
            if (distanceToPlayer <= attackRange && canAttack)
            {
                // Hy�kk�� jos pelaaja on kantamalla ja cooldown on valmis
                currentState = State.Attacking;
                int attackChoice = Random.Range(0, 2);
                if (attackChoice == 0)
                {
                    Debug.Log("ActionLoop: Triggering Attack 1");
                    animator.SetTrigger("Attack1Prep");
                }
                else
                {
                    Debug.Log("ActionLoop: Triggering Attack 2");
                    animator.SetTrigger("Attack2");
                }
                lastAttackTime = Time.time; // Nollaa cooldown
                // K�ynnistet��n korutiini valvomaan hy�kk�yksen p��ttymist�
                currentActionCoroutine = StartCoroutine(MonitorActionCompletion());
                yield break; // Poistu ActionLoopista, MonitorActionCompletion hoitaa jatkon
            }
            else if (distanceToPlayer > desiredStoppingDistance)
            {
                // K�vele jos pelaaja on liian kaukana
                currentState = State.Walking;
                Debug.Log("ActionLoop: Triggering Walk");
                animator.SetTrigger("Walk");
                currentActionCoroutine = StartCoroutine(WalkRoutine());
                yield break; // Poistu ActionLoopista, WalkRoutine hoitaa jatkon
            }
            else
            {
                // Pelaaja on l�hell�, mutta ei hy�kk�yset�isyydell� TAI cooldown ei ole valmis
                // Pysy Idless� ja odota seuraavaa p��t�st� (ActionLoop jatkuu)
                Debug.Log("ActionLoop: Player close but not attacking (range/cooldown). Staying IdleDecision.");
                // Varmista, ett� animaattori on idless�, jos se ei jo ollut
                if (!IsAnimatorInState(hashIdle)) animator.SetTrigger("Idle");
                // Ei yield break, annetaan while-silmukan jatkua ja odottaa seuraavaa kierrosta
                yield return null; // Odota frame ennen uutta tarkistusta
            }
        }
        Debug.Log("ActionLoop ended (likely Dead).");
    }

    // Korutiini k�velylle - Pys�htyy et�isyyden perusteella
    private IEnumerator WalkRoutine()
    {
        Debug.Log("WalkRoutine started.");
        while (currentState == State.Walking) // Poistuttiin ajastimesta
        {
            if (playerTransform == null) { yield break; } // Lopeta jos pelaaja katoaa

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Jos ollaan tarpeeksi l�hell�, lopeta k�vely
            if (distanceToPlayer <= desiredStoppingDistance)
            {
                Debug.Log("WalkRoutine: Reached desired stopping distance.");
                break; // Poistu while-silmukasta
            }

            // Liiku ja k��nny
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            MoveTowards(direction);
            // K��ntyminen hoidetaan nyt Updatessa
            // FlipTowards(direction); // Tai voit pit�� sen t�ss�kin

            yield return null; // Odota seuraavaa framea
        }

        // K�velyn lopetus (joko et�isyyden tai keskeytyksen vuoksi)
        if (currentState == State.Walking) // Vain jos k�vely loppui normaalisti
        {
            Debug.Log("WalkRoutine finished, returning to Idle decision.");
            animator.ResetTrigger("Walk"); // Varmista ettei j�� p��lle
            animator.SetTrigger("Idle");
            currentState = State.IdleDecision;
            StartActionLoop(); // K�ynnist� p��t�ksenteko uudelleen
        }
        else
        {
            Debug.Log($"WalkRoutine interrupted. Current state: {currentState}");
        }
    }

    // UUSI Korutiini: Valvoo hy�kk�yksen p��ttymist� (Animatorin paluuta Idleen)
    private IEnumerator MonitorActionCompletion()
    {
        Debug.Log("MonitorActionCompletion: Waiting for attack animation to return to Idle state...");

        // Odota kunnes animaattori EI ole en�� hy�kk�ysanimaatioissa JA on palannut Idleen
        // T�m� vaatii tarkempaa tietoa hy�kk�ysanimaatioiden nimist�/hasheista.
        // Yksinkertaistettu versio: Odota kunnes ollaan Idless�.
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Hurt || currentState == State.Dead);


        // Pieni lis�varmistus, ett� ollaan varmasti Idless� eik� juuri siirtym�ss�
        //yield return new WaitForSeconds(0.1f); // Valinnainen pieni viive

        if (currentState != State.Dead && currentState != State.Hurt)
        {
            // Varmistetaan uudelleen tila, jos se ei ole muuttunut
            if (currentState == State.Attacking || IsAnimatorInState(hashIdle)) // Joskus tila voi olla viel� Attacking vaikka animaattori on jo Idless�
            {
                Debug.Log("MonitorActionCompletion: Action presumed complete (Animator Idle or state not Hurt/Dead). Restarting Action Loop.");
                currentState = State.IdleDecision; // Aseta tila OIKEIN
                StartActionLoop();
            }
            else
            {
                Debug.Log($"MonitorActionCompletion: State changed unexpectedly during monitoring ({currentState}). Letting other logic handle.");
            }

        }
        else
        {
            Debug.Log($"MonitorActionCompletion: Interrupted by Hurt/Dead. State: {currentState}");
            // Hurt/Death logiikka hoitaa jo uudelleenk�ynnistyksen tai lopetuksen
        }
    }


    private void MoveTowards(Vector2 direction)
    {
        if (currentState != State.Walking) return;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
    }

    // K��ntymisfunktio, jota Update voi kutsua
    private void FlipTowardsPlayer()
    {
        if (playerTransform == null || currentState == State.Dead) return;
        float playerDirectionX = playerTransform.position.x - transform.position.x;
        if (playerDirectionX > 0.1f && !isFacingRight)
        {
            // Katso oikealle
            isFacingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (playerDirectionX < -0.1f && isFacingRight)
        {
            // Katso vasemmalle
            isFacingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // --- Vahinko ja Kuolema (ennallaan, mutta tarkista StartActionLoop kutsu RecoverFromHurt:ssa) ---
    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");

        // Pys�ytet��n nykyinen toimintokorutiini
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = State.Hurt;
            animator.SetTrigger("Hurt");
            Debug.Log("Triggering Hurt");
            // K�ynnistet��n toipuminen
            currentActionCoroutine = StartCoroutine(RecoverFromHurt());
        }
    }

    private IEnumerator RecoverFromHurt()
    {
        Debug.Log("RecoverFromHurt: Waiting for Hurt animation...");
        // Odotetaan kunnes Hurt-animaatio on ohi JA ollaan palattu Idleen
        // T�m� vaatii ett� Hurt -> Idle siirtym�ll� on Has Exit Time p��ll�
        yield return new WaitUntil(() => IsAnimatorInState(hashIdle) || currentState == State.Dead);

        if (currentState != State.Dead)
        {
            Debug.Log("Recovered from Hurt, restarting Action Loop.");
            // Ei tarvitse asettaa currentState = State.IdleDecision t�ss�,
            // koska StartActionLoop tekee sen.
            StartActionLoop();
        }
        else
        {
            Debug.Log("RecoverFromHurt: Died during hurt recovery.");
        }
    }

    private void Die()
    {
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        StopAllCoroutines(); // Varmuuden vuoksi

        Debug.Log("Triggering Death");
        currentState = State.Dead;
        // Varmista ettei mik��n triggeri j�� p��lle
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Attack1Prep");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Hurt");
        animator.SetTrigger("Death");

        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Pys�yt� liike jos k�yt�t rigidbodya
            GetComponent<Rigidbody2D>().simulated = false;
        }
        Destroy(gameObject, 3f);
    }

    private bool IsAnimatorInState(int stateHash)
    {
        // Lis�t��n tarkistus, onko animaattori edes aktiivinen/initialisoitu
        if (animator == null || !animator.isInitialized || !animator.gameObject.activeInHierarchy)
        {
            // Debug.LogWarning("IsAnimatorInState check failed: Animator not ready or inactive.");
            return false;
        }
        // Varmista ett� k�yt�t oikeaa layer indexi� (yleens� 0)
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
    }

    // Apufunktio debuggaukseen (valinnainen)
    string GetCurrentAnimatorStateName()
    {
        if (animator == null || !animator.isInitialized || !animator.gameObject.activeInHierarchy) return "Animator_Not_Ready";
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // T�m� vaatii, ett� tied�t animaatioiden nimet tarkasti
        if (stateInfo.IsName("Idle")) return "Idle";
        if (stateInfo.IsName("Walk")) return "Walk";
        if (stateInfo.IsName("Attack1_prep")) return "Attack1_prep";
        if (stateInfo.IsName("Attack1Stage2")) return "Attack1Stage2";
        if (stateInfo.IsName("Attack1FinalStage")) return "Attack1FinalStage";
        if (stateInfo.IsName("Attack2")) return "Attack2";
        if (stateInfo.IsName("Hurt")) return "Hurt";
        if (stateInfo.IsName("Death")) return "Death";
        return "Unknown_" + stateInfo.shortNameHash;
    }
}