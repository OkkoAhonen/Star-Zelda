using UnityEngine;
using System.Collections; // Tarvitaan Coroutineja varten

public class NPCController : MonoBehaviour
{
    public float moveSpeed = 2f;         // NPC:n k�velynopeus
    public float walkLeftDuration = 3f;  // Aika sekunneissa, jonka NPC k�velee vasemmalle
    public float stopDuration = 1.5f;    // Aika sekunneissa, jonka NPC pys�htyy
    public float walkRightDuration = 3f; // Aika sekunneissa, jonka NPC k�velee oikealle

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isFacingRight = true; // Oletetaan, ett� sprite katsoo oletuksena oikealle
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D-komponenttia ei l�ydy NPC-objektista! Lis�� se.");
            enabled = false; // Est� skriptin suoritus, jos komponentti puuttuu
            return;
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer-komponenttia ei l�ydy NPC-objektista! Lis�� se.");
            enabled = false; // Est� skriptin suoritus, jos komponentti puuttuu
            return;
        }

        // Varmista, ett� NPC ei py�ri fysiikan vaikutuksesta Z-akselilla


        // Aloita k�ytt�ytymisjakso
        StartCoroutine(PatrolSequence());
    }

    

    IEnumerator PatrolSequence()
    {
        // --- VAIHE 1: K�vele vasemmalle ---
        Debug.Log("NPC: K�velee vasemmalle");
        if (isFacingRight) // Jos katsoo oikealle, k��nny vasemmalle
        {
            Flip();
        }
        // Aseta nopeus vasemmalle
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        // Odota m��ritetty aika
        yield return new WaitForSeconds(walkLeftDuration);

        // --- VAIHE 2: Pys�hdy ---
        Debug.Log("NPC: Pys�htyy (vasemman j�lkeen)");
        // Pys�yt� liike
        rb.linearVelocity = Vector2.zero;
        // Odota m��ritetty aika
        yield return new WaitForSeconds(stopDuration);

        // --- VAIHE 3: K��nny ja k�vele oikealle ---
        Debug.Log("NPC: K�velee oikealle");
        if (!isFacingRight) // Jos katsoo vasemmalle, k��nny oikealle
        {
            Flip();
        }
        // Aseta nopeus oikealle
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        // Odota m��ritetty aika
        yield return new WaitForSeconds(walkRightDuration);

        // --- VAIHE 4: Pys�hdy lopullisesti ---
        Debug.Log("NPC: Pys�htyy (oikean j�lkeen) - sekvenssi valmis");
        // Pys�yt� liike
        rb.linearVelocity = Vector2.zero;

        // T�h�n voisi lis�t� logiikkaa, jos haluat NPC:n jatkavan jotain muuta
        // Esimerkiksi aloittavan sekvenssin alusta:
        // StartCoroutine(PatrolSequence());
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        // Tapa 1: K��nn� SpriteRendererin flipX-ominaisuutta
        // T�m� on yleens� suositeltavin tapa 2D-spriteille
        spriteRenderer.flipX = !isFacingRight; // Jos sprite oletuksena katsoo oikealle

        // Tapa 2: K��nn� koko GameObjectin skaalaus (vaikuttaa my�s lapsiobjekteihin)
        // Vector3 theScale = transform.localScale;
        // theScale.x *= -1;
        // transform.localScale = theScale;

        Debug.Log("NPC: K��ntyi. Katsoo nyt " + (isFacingRight ? "oikealle" : "vasemmalle"));
    }

    // Update-metodia ei v�ltt�m�tt� tarvita t�ss� perusversiossa,
    // mutta se voisi olla hy�dyllinen esim. animaatioiden ohjaukseen.
    // void Update()
    // {
    //     // Esim. jos haluat animaattorin p�ivitt�v�n k�velyanimaatiota:
    //     // if (animator != null)
    //     // {
    //     //     animator.SetBool("isWalking", rb.velocity.x != 0);
    //     // }
    // }
}