using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionAttack : MonoBehaviour
{
    // PIDÄ TÄMÄ: Tarvitaan edelleen, jotta voidaan kertoa heitettävälle potionille,
    // mikä efekti sen pitää luoda osuessaan.
    [Header("Efekti Prefab")]
    [SerializeField] private GameObject potionSpash; // Tämä on se roiske/efekti, EI lentävä potion

    [Header("Heitettävä Prefab")]
    [Tooltip("Raahaa tähän se UUSI prefab, jossa on Sprite, Rigidbody2D ja ThrowablePotionController")]
    [SerializeField] private GameObject throwablePotionPrefab; // Tämä on se lentävä objekti

    [Header("Heiton Asetukset")]
    [Tooltip("Piste pelaajassa, josta heitto lähtee (esim. käsi). Jos tyhjä, lähtee keskeltä.")]
    [SerializeField] private Transform launchPoint;
    [Tooltip("Voima, jolla potion heitetään.")]
    [SerializeField] private float launchForce = 15f;

    [Header("Cooldown")]
    // Nimesin tämän uudelleen selkeyden vuoksi, voit pitää vanhan nimen
    [SerializeField] private float potionCooldown = 2f; // Käytä tätä tai item.potionActivetimer
    private float timer = 0f;

    // POISTA TAI KOMMENTOI TÄMÄ, jos et käytä sitä tässä skriptissä suoraan:
    // public float potionDamage = 20f;

    void Start() // Lisäsin Start-metodin varmistamaan, että timer on valmis alussa
    {
        timer = potionCooldown; // Aseta ajastin valmiiksi, jotta voi heittää heti
    }


    void Update()
    {
        // Päivitä ajastin ensin
        watchPotionTimer(); // Kutsutaan vain kerran per Update

        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

            // Ei tarvitse kutsua watchPotionTimer uudelleen täällä

            if (equippedItem.type == ItemType.potion)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Tarkista cooldown TÄSSÄ ennen heittoa
                    // Käytä joko yleistä cooldownia (potionCooldown) tai item-kohtaista (equippedItem.potionActivetimer)
                    if (timer >= potionCooldown) // Tai: timer >= equippedItem.potionActivetimer
                    {
                        potionAttack(equippedItem); // Annetaan item parametrina
                        timer = 0f; // Nollaa ajastin heiton jälkeen
                    }
                    else
                    {
                        Debug.Log($"Potion cooldown: {potionCooldown - timer:F1}s jäljellä");
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

        // Tarkistus: Onko heitettävä prefab asetettu?
        if (throwablePotionPrefab == null)
        {
            Debug.LogError("ThrowablePotionPrefab puuttuu PotionAttack-skriptistä! Aseta se Inspectorissa.");
            return;
        }
        // Tarkistus: Onko roiske-prefab asetettu? (Tarvitaan välittämään eteenpäin)
        if (potionSpash == null)
        {
            Debug.LogError("PotionSpash (efekti) prefab puuttuu PotionAttack-skriptistä! Aseta se Inspectorissa.");
            return;
        }


        // 1. Määritä lähtöpiste
        Vector3 startPos = (launchPoint != null) ? launchPoint.position : transform.position;

        // 2. Määritä kohdesijainti (hiiren paikka maailmassa)
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0f; // Varmista 2D

        // 3. Laske suunta lähtöpisteestä kohteeseen
        Vector2 direction = (targetPos - startPos).normalized;

        // 4. Luo (Instantiate) se UUSI lentävä potion-prefab lähtöpisteeseen
        GameObject potionInstance = Instantiate(throwablePotionPrefab, startPos, Quaternion.identity);

        // 5. Hae sen ThrowablePotionController -skripti
        ThrowablePotionController controller = potionInstance.GetComponent<ThrowablePotionController>();

        if (controller != null)
        {
            // 6. TÄRKEÄÄ: Anna kontrollerille tieto siitä, mikä roiske-prefab sen pitää luoda osuessaan
            // kontroller.potionSplashPrefab = this.potionSpash; // Tämä tehdään nyt Inspectorissa prefabille!
            // Jos haluaisit dynaamisesti vaihtaa roisketta itemin mukaan, asettaisit sen tässä.

            // 7. Käynnistä heitto antamalla suunta ja voima
            controller.Launch(direction, launchForce);

            Debug.Log($"Heitetty potion kohti {targetPos}");

            // 8. MUISTA KULUTTAA POTIONI INVENTORYSTA!
            // (Olettaen että sinulla on tähän metodi)
            //InventoryManager.Instance.UseSelectedItem(true);
        }
        else
        {
            Debug.LogError("Instansioidussa throwablePotionPrefabissa ei ollut ThrowablePotionController-skriptiä!");
            Destroy(potionInstance); // Tuhoa turha instanssi
        }

        // Nollaa ajastin (Siirretty Updateen onnistuneen heiton jälkeen)
        // timer = 0f;

        // --- UUSI KOODI PÄÄTTYY ---


        // --- VANHA KOODI POISTETAAN ---
        // Vector3 potionplace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // potionplace.z = 0f;
        // Instantiate(potionSpash, potionplace, Quaternion.identity); // Tämä korvattiin ylläolevalla
        // Debug.Log($"Potion spawned at: {potionplace}");
        // timer = 0f;
        // } // if (timer >= ...) loppu
    }

    public void watchPotionTimer()
    {
        // Kasvatetaan ajastinta vain jos se on pienempi kuin cooldown,
        // ettei se kasva loputtomiin.
        if (timer < potionCooldown) // Tai: timer < equippedItem.potionActivetimer (vaatii itemin välittämistä)
        {
            timer += Time.deltaTime;
        }
        // Varmistetaan, ettei timer ylitä cooldownia (valinnainen, mutta siistiä)
        // timer = Mathf.Min(timer, potionCooldown);
    }
}