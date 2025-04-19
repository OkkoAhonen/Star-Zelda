using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionAttack : MonoBehaviour
{
    // PID� T�M�: Tarvitaan edelleen, jotta voidaan kertoa heitett�v�lle potionille,
    // mik� efekti sen pit�� luoda osuessaan.
    [Header("Efekti Prefab")]
    [SerializeField] private GameObject potionSpash; // T�m� on se roiske/efekti, EI lent�v� potion

    [Header("Heitett�v� Prefab")]
    [Tooltip("Raahaa t�h�n se UUSI prefab, jossa on Sprite, Rigidbody2D ja ThrowablePotionController")]
    [SerializeField] private GameObject throwablePotionPrefab; // T�m� on se lent�v� objekti

    [Header("Heiton Asetukset")]
    [Tooltip("Piste pelaajassa, josta heitto l�htee (esim. k�si). Jos tyhj�, l�htee keskelt�.")]
    [SerializeField] private Transform launchPoint;
    [Tooltip("Voima, jolla potion heitet��n.")]
    [SerializeField] private float launchForce = 15f;

    [Header("Cooldown")]
    // Nimesin t�m�n uudelleen selkeyden vuoksi, voit pit�� vanhan nimen
    [SerializeField] private float potionCooldown = 2f; // K�yt� t�t� tai item.potionActivetimer
    private float timer = 0f;

    // POISTA TAI KOMMENTOI T�M�, jos et k�yt� sit� t�ss� skriptiss� suoraan:
    // public float potionDamage = 20f;

    void Start() // Lis�sin Start-metodin varmistamaan, ett� timer on valmis alussa
    {
        timer = potionCooldown; // Aseta ajastin valmiiksi, jotta voi heitt�� heti
    }


    void Update()
    {
        // P�ivit� ajastin ensin
        watchPotionTimer(); // Kutsutaan vain kerran per Update

        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

            // Ei tarvitse kutsua watchPotionTimer uudelleen t��ll�

            if (equippedItem.type == ItemType.potion)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Tarkista cooldown T�SS� ennen heittoa
                    // K�yt� joko yleist� cooldownia (potionCooldown) tai item-kohtaista (equippedItem.potionActivetimer)
                    if (timer >= potionCooldown) // Tai: timer >= equippedItem.potionActivetimer
                    {
                        potionAttack(equippedItem); // Annetaan item parametrina
                        timer = 0f; // Nollaa ajastin heiton j�lkeen
                    }
                    else
                    {
                        Debug.Log($"Potion cooldown: {potionCooldown - timer:F1}s j�ljell�");
                    }
                }
            }
        }
    }

    // Muutetaan ottamaan Item parametrina
    public void potionAttack(Item equippedItem)
    {
        // --- VANHA KOODI POISTETAAN ---
        // Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        // if (timer >= equippedItem.potionActivetimer) { ... } // Cooldown tarkistus siirretty Updateen

        // --- UUSI KOODI ALKAA ---

        // Tarkistus: Onko heitett�v� prefab asetettu?
        if (throwablePotionPrefab == null)
        {
            Debug.LogError("ThrowablePotionPrefab puuttuu PotionAttack-skriptist�! Aseta se Inspectorissa.");
            return;
        }
        // Tarkistus: Onko roiske-prefab asetettu? (Tarvitaan v�litt�m��n eteenp�in)
        if (potionSpash == null)
        {
            Debug.LogError("PotionSpash (efekti) prefab puuttuu PotionAttack-skriptist�! Aseta se Inspectorissa.");
            return;
        }


        // 1. M��rit� l�ht�piste
        Vector3 startPos = (launchPoint != null) ? launchPoint.position : transform.position;

        // 2. M��rit� kohdesijainti (hiiren paikka maailmassa)
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0f; // Varmista 2D

        // 3. Laske suunta l�ht�pisteest� kohteeseen
        Vector2 direction = (targetPos - startPos).normalized;

        // 4. Luo (Instantiate) se UUSI lent�v� potion-prefab l�ht�pisteeseen
        GameObject potionInstance = Instantiate(throwablePotionPrefab, startPos, Quaternion.identity);

        // 5. Hae sen ThrowablePotionController -skripti
        ThrowablePotionController controller = potionInstance.GetComponent<ThrowablePotionController>();

        if (controller != null)
        {
            // 6. T�RKE��: Anna kontrollerille tieto siit�, mik� roiske-prefab sen pit�� luoda osuessaan
            // kontroller.potionSplashPrefab = this.potionSpash; // T�m� tehd��n nyt Inspectorissa prefabille!
            // Jos haluaisit dynaamisesti vaihtaa roisketta itemin mukaan, asettaisit sen t�ss�.

            // 7. K�ynnist� heitto antamalla suunta ja voima
            controller.Launch(direction, launchForce);

            Debug.Log($"Heitetty potion kohti {targetPos}");

            // 8. MUISTA KULUTTAA POTIONI INVENTORYSTA!
            // (Olettaen ett� sinulla on t�h�n metodi)
            //InventoryManager.Instance.UseSelectedItem(true);
        }
        else
        {
            Debug.LogError("Instansioidussa throwablePotionPrefabissa ei ollut ThrowablePotionController-skripti�!");
            Destroy(potionInstance); // Tuhoa turha instanssi
        }

        // Nollaa ajastin (Siirretty Updateen onnistuneen heiton j�lkeen)
        // timer = 0f;

        // --- UUSI KOODI P��TTYY ---


        // --- VANHA KOODI POISTETAAN ---
        // Vector3 potionplace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // potionplace.z = 0f;
        // Instantiate(potionSpash, potionplace, Quaternion.identity); // T�m� korvattiin yll�olevalla
        // Debug.Log($"Potion spawned at: {potionplace}");
        // timer = 0f;
        // } // if (timer >= ...) loppu
    }

    public void watchPotionTimer()
    {
        // Kasvatetaan ajastinta vain jos se on pienempi kuin cooldown,
        // ettei se kasva loputtomiin.
        if (timer < potionCooldown) // Tai: timer < equippedItem.potionActivetimer (vaatii itemin v�litt�mist�)
        {
            timer += Time.deltaTime;
        }
        // Varmistetaan, ettei timer ylit� cooldownia (valinnainen, mutta siisti�)
        // timer = Mathf.Min(timer, potionCooldown);
    }
}