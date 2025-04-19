using UnityEngine;

public class ThrowablePotion : MonoBehaviour
{
    // Raahaa roiske-efektin Prefab t‰h‰n Inspectorissa
    public GameObject potionSplashPrefab;
    // Vahinko ja muut tiedot asetetaan PotionAttack-skriptist‰, kun potion heitet‰‰n
    public float damage;

    private Rigidbody2D rb;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("ThrowablePotion vaatii Rigidbody2D!");
    }

    void Start()
    {
        // Tuhoa itsesi automaattisesti 5 sekunnin kuluttua, jos et osu mihink‰‰n
        Destroy(gameObject, 5f);
    }

    // Metodi, jota PotionAttack kutsuu antaakseen vauhtia
    public void Launch(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }

    // T‰t‰ kutsutaan, kun t‰m‰ (ei-trigger) collider osuu toiseen collideriin
    void OnCollisionEnter2D(Collision2D collision)
    {
        // K‰ytet‰‰n tarkkaa osumapistett‰ roiskeen spawnaamiseen
        Vector3 impactPosition = collision.contacts[0].point;
        HandleImpact(impactPosition);
    }

    private void HandleImpact(Vector3 impactPosition)
    {
        if (hasHit) return; // Est‰‰ moninkertaiset osumat
        hasHit = true;

        Debug.Log($"Potion osui: {impactPosition}");

        // 1. Luo roiske-efekti osumakohtaan
        if (potionSplashPrefab != null)
        {
            GameObject splashInstance = Instantiate(potionSplashPrefab, impactPosition, Quaternion.identity);

            // --- Vahingon k‰sittely (jos roiske tekee sen) ---
            // Yrit‰ hakea roiskeesta skripti, joka jakaa vahingon
            PotionSplashEffect splashScript = splashInstance.GetComponent<PotionSplashEffect>();
            if (splashScript != null)
            {
                // Anna vahingon m‰‰r‰ roiske-skriptille
                splashScript.Initialize(damage);
            }
            else
            {
                // Jos roiskeella ei ole omaa skripti‰, vahinkoa ei (viel‰) jaeta.
                // Voitaisiin tehd‰ vahinko t‰ss‰kin, mutta parempi roiske-skriptiss‰.
                Debug.LogWarning("Roiske-prefabissa ei ollut PotionSplashEffect-skripti‰.");
            }
            // --- Vahingon k‰sittely p‰‰ttyy ---
        }
        else
        {
            Debug.LogError("PotionSplashPrefab ei ole asetettu ThrowablePotion-skriptiin!");
        }

        // 2. Tuhoa t‰m‰ lent‰v‰ potion-objekti
        Destroy(gameObject);
    }
}