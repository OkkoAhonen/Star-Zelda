using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Lisää tämä, jos haluat käyttää SceneManager.sceneLoaded -tapahtumaa tulevaisuudessa
// using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // Komponenttiviittaukset
    [Header("Core Components")]
    [SerializeField] private Rigidbody2D rb;
    private PlayerStats stats;
    private Camera cam;

    [Header("Rotation Fix (Optional)")]
    public GameObject rotationFix; // Objekti, jonka rotaatio halutaan pitää vakiona
    private Quaternion rotationFixRotation; // Tallentaa rotationFixin alkuperäisen rotaation

    // Liikkumisparametrit
    private Vector2 moveDirection; // Pelaajan syöttämä liikesuunta

    // Dash-parametrit
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;      // Dashin nopeus
    [SerializeField] private float dashDuration = 0.15f; // Dashin kesto sekunneissa
    [SerializeField] private float dashCooldown = 1f;    // Dashin cooldown sekunneissa

    // Dashin tilamuuttujat
    private bool isDashing = false;  // Onko pelaaja parhaillaan dashaamassa?
    private bool canDash = true;     // Voiko pelaaja dashata (ei cooldownilla)?
    private Coroutine dashCoroutine; // Viittaus käynnissä olevaan dash-korutiiniin

    // --- Unity Lifecycle Methods ---

    private void Awake()
    {
        // Hae viittaus kameraan heti alussa
        FindMainCamera();

        // Hae komponentit tästä GameObjectista
        // On yleensä turvallisempaa hakea komponentit Startissa tai Awakessa
        // Varmista, että ne ovat olemassa ennen käyttöä
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();

        if (stats == null)
        {
            Debug.LogError("PlayerStats-komponenttia ei löytynyt tästä GameObjectista!", this);
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D-komponenttia ei löytynyt tästä GameObjectista!", this);
        }
    }

    void Start()
    {
        // Tallennetaan rotationFixin alkurotaatio, jos se on asetettu
        if (rotationFix != null)
        {
            rotationFixRotation = rotationFix.transform.rotation;
        }
        else
        {
            Debug.LogWarning("rotationFix ei ole määritelty Inspectorissa!", this);
        }

        // Varmistetaan alkutila
        canDash = true;
        isDashing = false;
    }

    // OnEnable kutsutaan aina, kun skripti aktivoituu:
    // 1. Kun peli käynnistyy ja objekti on aktiivinen scenessä.
    // 2. Kun objekti luodaan Instantiate()-kutsulla.
    // 3. Kun objekti aktivoidaan SetActive(true)-kutsulla.
    // 4. TÄRKEÄÄ: Jos objekti käyttää DontDestroyOnLoad, OnEnable kutsutaan
    //    uudelleen scenen vaihtuessa (jos objekti oli deaktivoituna välissä tai Unityn sisäisen toiminnan vuoksi).
    // Tämä on hyvä paikka nollata tila ja hakea scenekohtaisia viittauksia.
    void OnEnable()
    {
        Debug.Log("PlayerMovement Enabled - Resetting State and Finding Camera");
        ResetDashState(); // Nollaa dashin tila (erityisen tärkeää scenen vaihdon jälkeen)
        FindMainCamera(); // Yritä löytää (mahdollisesti uuden scenen) pääkamera
    }

    void Update()
    {
        // Jos komponentteja puuttuu, älä jatka
        if (rb == null || stats == null) return;

        // --- Inputien lukeminen ---
        ProcessInputs();

        // --- Liikkumisen suoritus ---
        // Liikutetaan pelaajaa normaalisti VAIN jos hän ei ole dashaamassa
        if (!isDashing)
        {
            ApplyMovement();
        }

        // --- Rotaatio ---
        // Varmistetaan, että kamera on löytynyt ennen rotaation yrittämistä
        if (cam != null)
        {
            PlayerMouseRotation();
        }
        else
        {
            // Yritetään löytää kamera uudelleen, jos se puuttuu
            FindMainCamera();
        }
    }

    private void LateUpdate()
    {
        // Pidetään rotationFixin rotaatio vakiona (jos asetettu)
        if (rotationFix != null)
        {
            rotationFix.transform.rotation = rotationFixRotation;
        }
    }

    // --- Input & Movement Logic ---

    private void ProcessInputs()
    {
        // Tavallinen liikkuminen
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");
        moveDirection = new Vector2(x, y).normalized;

        // Dash-input
        // Tarkistetaan, painetaanko Left Shift JA voiko pelaaja dashata (ei cooldownilla ja ei jo dashaamassa)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            // Pysäytä mahdollinen vanha korutiini (varmuuden vuoksi)
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            // Aloita uusi dash-korutiini ja tallenna viittaus siihen
            dashCoroutine = StartCoroutine(Dash());
        }
    }

    // Metodi normaalin liikkeen soveltamiseen
    void ApplyMovement()
    {
        // Käytetään PlayerStatsista saatua nopeutta
        rb.linearVelocity = moveDirection * stats.playerMoveSpeed;
    }

    // Metodi pelaajan kääntämiseksi hiiren osoittimen suuntaan
    public void PlayerMouseRotation()
    {
        // Ei tehdä mitään, jos kameraa ei ole
        if (cam == null) return;

        Vector3 mousepos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousepos.z = 0; // Varmistetaan, että ollaan 2D-tasossa

        Vector3 directionToMouse = mousepos - transform.position;

        // Varmistetaan, että emme yritä laskea kulmaa samaan pisteeseen (vältetään Atan2(0,0))
        if (directionToMouse.sqrMagnitude > 0.01f)
        {
            float angleRad = Mathf.Atan2(directionToMouse.y, directionToMouse.x);
            float angleDeg = angleRad * Mathf.Rad2Deg; // Käytä Mathf.Rad2Deg -vakiota

            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }
        // Debuggausviiva hiiren sijaintiin (voi poistaa kommentoinnin tarvittaessa)
        // Debug.DrawLine(transform.position, mousepos, Color.red);
    }


    // --- Dash Logic ---

    // Korutiini hoitaa dashin keston ja cooldownin
    private IEnumerator Dash()
    {
        Debug.Log("Dash Started!");
        canDash = false;    // Estetään uusi dash heti
        isDashing = true;   // Merkitään pelaaja dashaavaksi

        // Tallennetaan alkuperäinen painovoima (jos käytössä) ja poistetaan se dashin ajaksi
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Määritetään dashin suunta
        Vector2 dashDirection = moveDirection;
        // Jos pelaaja ei liiku (moveDirection on nolla), dashataan siihen suuntaan mihin hän katsoo
        if (dashDirection == Vector2.zero)
        {
            // transform.right osoittaa pelaajan paikalliseen oikeaan suuntaan (sprite/modelin eteenpäin)
            dashDirection = transform.right;
        }

        // Asetetaan dash-nopeus Rigidbodyyn
        rb.linearVelocity = dashDirection.normalized * dashSpeed;

        // Odotetaan dashin keston ajan
        yield return new WaitForSeconds(dashDuration);

        // --- Dashin jälkeiset toimet ---
        rb.linearVelocity = Vector2.zero; // Pysäytä liike heti dashin jälkeen (valinnainen, voi poistaa)
        rb.gravityScale = originalGravity; // Palautetaan alkuperäinen painovoima
        isDashing = false; // Pelaaja ei enää dashaa

        Debug.Log("Dash Finished. Cooldown Started.");

        // Odotetaan cooldownin ajan
        yield return new WaitForSeconds(dashCooldown);

        // --- Cooldownin jälkeiset toimet ---
        canDash = true; // Pelaaja voi taas dashata
        dashCoroutine = null; // Nollataan korutiiniviittaus, kun se on valmis
        Debug.Log("Dash Cooldown Finished. Can dash again.");
    }

    // Metodi nollaamaan dashin tila (kutsutaan esim. OnEnable-metodista)
    private void ResetDashState()
    {
        // Pysäytä mahdollisesti käynnissä oleva dash/cooldown korutiini
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }
        // Varmuuden vuoksi pysäytä kaikki tämän skriptin korutiinit
        // Ole varovainen, jos tällä skriptillä on muita tärkeitä korutiineja
        // StopAllCoroutines();

        isDashing = false; // Ei olla dashaamassa
        canDash = true;    // Voidaan dashata heti (cooldown nollataan)

        // Nollaa nopeus varmuuden vuoksi, jos Rigidbody on jo olemassa
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // Voit myös harkita painovoiman palauttamista oletusarvoon täällä,
            // jos tiedät sen vakioarvon (esim. rb.gravityScale = 1f;)
        }
        Debug.Log("Dash state has been reset.");
    }


    // --- Helper Methods ---

    // Metodi pääkameran löytämiseksi ja tallentamiseksi
    void FindMainCamera()
    {
        // Älä hae uudelleen, jos kamera on jo löytynyt
        // if (cam != null) return; // Voit lisätä tämän optimointina, mutta uudelleenhaku OnEnable:ssa voi olla tarpeen scenen vaihtuessa

        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Pääkameraa (tag: 'MainCamera') ei löytynyt scenestä!", this);
            // Voit yrittää löytää sen muilla tavoilla, jos tagia ei ole asetettu:
            // cam = FindObjectOfType<Camera>();
            // if (cam != null) Debug.Log("Löytyi kamera ilman 'MainCamera' tagia.");
        }
        else
        {
            Debug.Log("Main Camera Found.", this);
        }
    }
}