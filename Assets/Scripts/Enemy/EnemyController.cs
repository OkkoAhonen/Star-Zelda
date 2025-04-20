using UnityEngine;
using System.Collections; // Tarvitaan ehk� my�hemmin

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // Varmistaa, ett� Collider on olemassa
public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    public Enemy enemyStats; // Ved� Enemy ScriptableObject t�h�n Inspectorissa
    [SerializeField] private GameObject lootPrefab; // Ved� Loot Prefab t�h�n
    [SerializeField] private Item dropItem; // Valitse pudotettava Item

    [Header("Runtime References")]
    private Transform playerTransform;
    private Rigidbody2D rb;
    private playerAction playerActionScript; // Oletetaan, ett� pelaajalla on t�llainen skripti

    [Header("State")]
    private float currentHealth;
    private bool isPlayerInDetectionRange = false;

    void Start()
    {
        // ---- Alustukset ----
        if (enemyStats == null)
        {
            Debug.LogError($"EnemyStats puuttuu GameObjectilta: {gameObject.name}. Liit� Enemy ScriptableObject Inspectorissa.", this);
            enabled = false; // Poista skripti k�yt�st�
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        // Isaac-tyylisess� peliss� yleens� ei haluta gravitaatiota ja py�rimist�
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            // Haetaan pelaajan skripti valmiiksi, jotta voidaan tehd� vahinkoa
            playerActionScript = playerObject.GetComponent<playerAction>();
            if (playerActionScript == null)
            {
                Debug.LogWarning($"PlayerAction-skripti� ei l�ytynyt pelaajasta ({playerObject.name}). Vihollinen ei voi tehd� vahinkoa.", this);
            }
        }
        else
        {
            Debug.LogError("Pelaajaa (tagi 'Player') ei l�ytynyt scenest�.", this);
            enabled = false; // Poista skripti k�yt�st�
            return;
        }

        currentHealth = enemyStats.maxHealth;
    }

    void Update()
    {
        // ---- Logiikka joka ei vaadi fysiikkap�ivityst� ----
        CheckPlayerDistance();
        WatchHealth(); // Tarkkaile el�m�pisteit�
    }

    void FixedUpdate()
    {
        // ---- Fysiikkaan liittyv� logiikka ----
        if (isPlayerInDetectionRange && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
        // Jos et halua vihollisen liukuvan, voit asettaa velocityn nollaan t�ss�,
        // kun pelaaja ei ole l�hell�, mutta MovePositionin k�ytt�minen yleens� hoitaa t�m�n.
        // else { rb.velocity = Vector2.zero; }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null)
        {
            isPlayerInDetectionRange = false;
            return;
        }
        // K�ytet��n EnemyStatsista haettua rangea
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInDetectionRange = distanceToPlayer <= enemyStats.detectionRange;
    }

    void MoveTowardsPlayer()
    {
        // Laske suunta pelaajaan
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

        // Liikuta Rigidbody� kohti pelaajaa k�ytt�en EnemyStatsin nopeutta
        Vector2 targetPosition = rb.position + direction * enemyStats.speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // K��ntyminen (valinnainen, jos sprite vaatii):
        // Jos haluat spriten k��ntyv�n liikkeen suuntaan:
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // rb.rotation = angle; // Tai transform.eulerAngles jos et halua k�ytt�� rigidbody rotaatiota
        // Huom: Voi vaatia spriten oletusorientaation s��t��.
    }

    // Kutsutaan kun vihollisen Collider2D (jolla EI ole 'Is Trigger' p��ll�)
    // t�rm�� toiseen Collider2D:hen.
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Tarkista, osuiko pelaajaan k�ytt�m�ll� Tagia
        if (collision.gameObject.CompareTag("Player"))
        {
            // Tarkista, onko pelaajan skripti l�ydetty
            if (playerActionScript != null)
            {
                // Tee vahinkoa pelaajalle EnemyStatsista haetulla voimalla
                playerActionScript.playerTakeDamage(enemyStats.strength);
                // T�ss� voisi lis�t� pienen knockbackin tai lyhyen cooldownin vahingolle,
                // ettei vahinko tapahdu joka ikinen fysiikkakontakti-frame.
            }
            else
            {
                // Yrit� hakea skripti uudelleen, jos se oli alussa null
                playerActionScript = collision.gameObject.GetComponent<playerAction>();
                if (playerActionScript != null)
                {
                    playerActionScript.playerTakeDamage(enemyStats.strength);
                }
                else
                {
                    Debug.LogWarning($"Ei voitu tehd� vahinkoa: PlayerAction-skripti� ei l�ytynyt pelaajasta ({collision.gameObject.name}).", this);
                }
            }
        }
    }

    // Julkinen metodi, jota muut skriptit (esim. pelaajan ammus) voivat kutsua
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        // Debug.Log($"{gameObject.name} otti {damage} vahinkoa, el�m�� j�ljell�: {currentHealth}");

        // T�h�n voisi lis�t� v�l�hdyksen, ��nen tai muun palautteen
        // StartCoroutine(FlashRed());
    }

    void WatchHealth()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Est� useampi kuolema
        if (currentHealth <= -1000) return; // Varmistus
        currentHealth = -1001; // Merkit��n varmasti kuolleeksi

        // Pudota tavara, jos prefab ja item on m��ritelty
        if (lootPrefab != null && dropItem != null)
        {
            GameObject lootObject = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            Loot loot = lootObject.GetComponent<Loot>(); // Olettaen, ett� Loot-prefabissa on Loot-skripti
            if (loot != null)
            {
                // Olettaen, ett� Loot-skriptiss� on Initialize-metodi
                loot.Initialize(dropItem);
            }
            else
            {
                Debug.LogWarning($"Loot Prefabilla ({lootPrefab.name}) ei ole Loot-skripti�.", this);
            }
        }

        // T�ss� voisi olla kuolinanimaatio, efektit, ��net jne.
        // Esim. Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Tuhoa vihollinen scenest�
        Destroy(gameObject);
    }

    // Visualisoidaan havaintoet�isyys editorissa, haetaan arvo statsista
    void OnDrawGizmosSelected()
    {
        if (enemyStats != null)
        {
            Gizmos.color = Color.yellow; // Muuta v�ri� erottuvuuden vuoksi
            Gizmos.DrawWireSphere(transform.position, enemyStats.detectionRange);
        }
        else
        {
            // Jos statseja ei ole viel� asetettu, n�yt� oletusarvo tai ei mit��n
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 5f); // N�yt� esim. 5f oletuksena
        }
    }

    // Esimerkki v�l�hdysefektist� (valinnainen)
    /*
    IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(); // Tai GetComponent<SpriteRenderer>()
        if (sr != null) {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f); // V�l�hdyksen kesto
            sr.color = originalColor;
        }
    }
    */
}