using UnityEngine;

public class ThrowablePotionController : MonoBehaviour
{
    // Tarvitaan viittaus siihen roiske-efektiin, joka luodaan osumasta
    // Raahaa alkuper‰inen "potionSpash"-prefab t‰h‰n Inspectorissa
    [SerializeField] private GameObject potionSplashPrefab;

    private Rigidbody2D rb;
    private bool hasHit = false; // Est‰‰ efektin luomisen monta kertaa

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ThrowablePotionController vaatii Rigidbody2D-komponentin!");
        }
    }

    void Start()
    {
        // (Valinnainen) Tuhoa itsesi tietyn ajan kuluttua, jos et osu mihink‰‰n
        Destroy(gameObject, 5f); // Esim. 5 sekunnin kuluttua
    }

    // T‰m‰ metodi kutsutaan PotionAttack-skriptist‰ antamaan potku
    public void Launch(Vector2 direction, float force)
    {
        if (rb != null)
        {
            // Lis‰t‰‰n impulssivoima, joka saa sen lent‰m‰‰n
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

            // (Valinnainen) Lis‰‰ pient‰ pyˆrimist‰ heittoon
            // rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
        }
    }

    // T‰t‰ kutsutaan automaattisesti, kun t‰m‰n objektin collider osuu TOISEEN collideriin
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Varmistetaan, ettei t‰t‰ suoriteta useampaan kertaan, jos tulee monta osumaa nopeasti
        if (hasHit)
        {
            return;
        }
        hasHit = true;

        Debug.Log($"ThrowablePotion osui kohteeseen: {collision.gameObject.name} pisteess‰ {collision.contacts[0].point}");

        // 1. Spawnaa se ALKUPERƒINEN roiske-efekti (potionSpash) osumakohtaan
        if (potionSplashPrefab != null)
        {
            // K‰ytet‰‰n tarkkaa ensimm‰ist‰ osumapistett‰
            Vector3 impactPoint = collision.contacts[0].point;
            Instantiate(potionSplashPrefab, impactPoint, Quaternion.identity);
        }
        else
        {
            Debug.LogError("PotionSplashPrefab ei ole asetettu ThrowablePotionControlleriin!");
        }

        // 2. Tuhoa t‰m‰ lent‰v‰ potion-objekti
        Destroy(gameObject);
    }
}