using UnityEngine;
using System.Collections; // Tarvitaan ehkä myöhemmin

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // Varmistaa, että Collider on olemassa
public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    public Enemy enemyStats; // Vedä Enemy ScriptableObject tähän Inspectorissa
    [SerializeField] private GameObject lootPrefab; // Vedä Loot Prefab tähän
    [SerializeField] private Item dropItem; // Valitse pudotettava Item

    [Header("Runtime References")]
    private Transform playerTransform;
    private Rigidbody2D rb;
    private playerAction playerActionScript; // Oletetaan, että pelaajalla on tällainen skripti

    [Header("State")]
    private float currentHealth;
    private bool isPlayerInDetectionRange = false;

    void Start()
    {
        // ---- Alustukset ----
        if (enemyStats == null)
        {
            Debug.LogError($"EnemyStats puuttuu GameObjectilta: {gameObject.name}. Liitä Enemy ScriptableObject Inspectorissa.", this);
            enabled = false; // Poista skripti käytöstä
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        // Isaac-tyylisessä pelissä yleensä ei haluta gravitaatiota ja pyörimistä
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            // Haetaan pelaajan skripti valmiiksi, jotta voidaan tehdä vahinkoa
            playerActionScript = playerObject.GetComponent<playerAction>();
            if (playerActionScript == null)
            {
                Debug.LogWarning($"PlayerAction-skriptiä ei löytynyt pelaajasta ({playerObject.name}). Vihollinen ei voi tehdä vahinkoa.", this);
            }
        }
        else
        {
            Debug.LogError("Pelaajaa (tagi 'Player') ei löytynyt scenestä.", this);
            enabled = false; // Poista skripti käytöstä
            return;
        }

        currentHealth = enemyStats.maxHealth;
    }

    void Update()
    {
        // ---- Logiikka joka ei vaadi fysiikkapäivitystä ----
        CheckPlayerDistance();
        WatchHealth(); // Tarkkaile elämäpisteitä
    }

    void FixedUpdate()
    {
        // ---- Fysiikkaan liittyvä logiikka ----
        if (isPlayerInDetectionRange && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
        // Jos et halua vihollisen liukuvan, voit asettaa velocityn nollaan tässä,
        // kun pelaaja ei ole lähellä, mutta MovePositionin käyttäminen yleensä hoitaa tämän.
        // else { rb.velocity = Vector2.zero; }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null)
        {
            isPlayerInDetectionRange = false;
            return;
        }
        // Käytetään EnemyStatsista haettua rangea
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInDetectionRange = distanceToPlayer <= enemyStats.detectionRange;
    }

    void MoveTowardsPlayer()
    {
        // Laske suunta pelaajaan
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

        // Liikuta Rigidbodyä kohti pelaajaa käyttäen EnemyStatsin nopeutta
        Vector2 targetPosition = rb.position + direction * enemyStats.speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // Kääntyminen (valinnainen, jos sprite vaatii):
        // Jos haluat spriten kääntyvän liikkeen suuntaan:
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // rb.rotation = angle; // Tai transform.eulerAngles jos et halua käyttää rigidbody rotaatiota
        // Huom: Voi vaatia spriten oletusorientaation säätöä.
    }

    // Kutsutaan kun vihollisen Collider2D (jolla EI ole 'Is Trigger' päällä)
    // törmää toiseen Collider2D:hen.
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Tarkista, osuiko pelaajaan käyttämällä Tagia
        if (collision.gameObject.CompareTag("Player"))
        {
            // Tarkista, onko pelaajan skripti löydetty
            if (playerActionScript != null)
            {
                // Tee vahinkoa pelaajalle EnemyStatsista haetulla voimalla
                playerActionScript.playerTakeDamage(enemyStats.strength);
                // Tässä voisi lisätä pienen knockbackin tai lyhyen cooldownin vahingolle,
                // ettei vahinko tapahdu joka ikinen fysiikkakontakti-frame.
            }
            else
            {
                // Yritä hakea skripti uudelleen, jos se oli alussa null
                playerActionScript = collision.gameObject.GetComponent<playerAction>();
                if (playerActionScript != null)
                {
                    playerActionScript.playerTakeDamage(enemyStats.strength);
                }
                else
                {
                    Debug.LogWarning($"Ei voitu tehdä vahinkoa: PlayerAction-skriptiä ei löytynyt pelaajasta ({collision.gameObject.name}).", this);
                }
            }
        }
    }

    // Julkinen metodi, jota muut skriptit (esim. pelaajan ammus) voivat kutsua
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        // Debug.Log($"{gameObject.name} otti {damage} vahinkoa, elämää jäljellä: {currentHealth}");

        // Tähän voisi lisätä välähdyksen, äänen tai muun palautteen
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
        // Estä useampi kuolema
        if (currentHealth <= -1000) return; // Varmistus
        currentHealth = -1001; // Merkitään varmasti kuolleeksi

        // Pudota tavara, jos prefab ja item on määritelty
        if (lootPrefab != null && dropItem != null)
        {
            GameObject lootObject = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            Loot loot = lootObject.GetComponent<Loot>(); // Olettaen, että Loot-prefabissa on Loot-skripti
            if (loot != null)
            {
                // Olettaen, että Loot-skriptissä on Initialize-metodi
                loot.Initialize(dropItem);
            }
            else
            {
                Debug.LogWarning($"Loot Prefabilla ({lootPrefab.name}) ei ole Loot-skriptiä.", this);
            }
        }

        // Tässä voisi olla kuolinanimaatio, efektit, äänet jne.
        // Esim. Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Tuhoa vihollinen scenestä
        Destroy(gameObject);
    }

    // Visualisoidaan havaintoetäisyys editorissa, haetaan arvo statsista
    void OnDrawGizmosSelected()
    {
        if (enemyStats != null)
        {
            Gizmos.color = Color.yellow; // Muuta väriä erottuvuuden vuoksi
            Gizmos.DrawWireSphere(transform.position, enemyStats.detectionRange);
        }
        else
        {
            // Jos statseja ei ole vielä asetettu, näytä oletusarvo tai ei mitään
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 5f); // Näytä esim. 5f oletuksena
        }
    }

    // Esimerkki välähdysefektistä (valinnainen)
    /*
    IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(); // Tai GetComponent<SpriteRenderer>()
        if (sr != null) {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f); // Välähdyksen kesto
            sr.color = originalColor;
        }
    }
    */
}