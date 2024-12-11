using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Viittaus miekan GameObjectiin, jota käytetään lyöntianimaatioihin.
    public GameObject sword;

    // Nopeus, jolla miekkaa heilautetaan (asteet sekunnissa).
    public float swingSpeed = 200f;

    // Tieto siitä, onko miekka parhaillaan heilautuksessa.
    private bool isSwinging = false;

    // Lyönnin suunta (oletuksena oikealle).
    private Vector2 attackDirection = Vector2.right;

    // Määriteltyjä Transform-pisteitä, joihin miekka siirtyy eri hyökkäyssuunnissa.
    [SerializeField] private Transform UpSwing;
    [SerializeField] private Transform DownSwing;
    [SerializeField] private Transform LeftSwing;
    [SerializeField] private Transform RightSwing;

    // Miekan liikkeen asetukset, jotka voidaan säätää Unityn Inspectorissa.
    [Header("Swing Settings")]
    [Tooltip("Miekan liikkuma etäisyys (yksiköitä) heilautuksen aikana.")]
    public float swingDistance = 2.0f;

    [Tooltip("Aika (sekunteina), joka menee heilautukseen eteenpäin ja takaisin.")]
    public float swingDuration = 0.5f;

    // Miekan heilautuksen kulkusuunta.
    private Vector3 swingEndPosition;

    void Start()
    {
        // Tarkistetaan, että miekka on määritetty, ja annetaan varoitus, jos ei ole.
        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned to PlayerCombat!");
        }
    }

    void Update()
    {
        // Päivittää hyökkäyssuunnan nuolinäppäimien perusteella.
        HandleAttackDirection();

        // Aloittaa miekan heilautuksen, jos välilyöntiä painetaan eikä miekka ole jo heilautuksessa.
        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartCoroutine(SwingSword());
        }
    }

    void HandleAttackDirection()
    {
        // Päivittää hyökkäyssuunnan nuolinäppäimien perusteella.
        if (Input.GetKey(KeyCode.UpArrow))
        {
            attackDirection = Vector2.up;
            sword.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            attackDirection = Vector2.down;
            sword.transform.rotation = Quaternion.Euler(0, 0, -180);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            attackDirection = Vector2.left;
            sword.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            attackDirection = Vector2.right;
            sword.transform.rotation = Quaternion.Euler(0, 0, -90);
        }

        // Päivittää miekan sijainnin hyökkäyssuunnan mukaan.
        UpdateSwordPosition();
    }

    void UpdateSwordPosition()
    {
        // Siirtää miekan sijainnin oikeaan kohtaan määritettyjen Transform-pisteiden perusteella.
        if (attackDirection == Vector2.up)
        {
            sword.transform.position = UpSwing.position;
        }
        else if (attackDirection == Vector2.down)
        {
            sword.transform.position = DownSwing.position;
        }
        else if (attackDirection == Vector2.left)
        {
            sword.transform.position = LeftSwing.position;
        }
        else if (attackDirection == Vector2.right)
        {
            sword.transform.position = RightSwing.position;
        }
    }

    IEnumerator SwingSword()
    {
        // Merkitsee, että miekka on heilautuksessa.
        isSwinging = true;

        // Miekan alkuperäinen sijainti.
        Vector3 initialPosition = sword.transform.position;

        // Määritetään, mihin suuntaan miekka liikkuu.
        swingEndPosition = initialPosition + (Vector3)attackDirection * swingDistance;

        // Liikkeen nopeus ja aikaraja.
        float elapsedTime = 0f;

        // Liikutetaan miekkaa eteenpäin.
        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(initialPosition, swingEndPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Varmistetaan, että miekka on päässyt täyteen etäisyyteensä.
        sword.transform.position = swingEndPosition;

        // Palautetaan miekka alkuperäiseen paikkaansa.
        elapsedTime = 0f;
        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(swingEndPosition, initialPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asetetaan miekan sijainti alkuperäiseen tilaan.
        sword.transform.position = initialPosition;

        // Merkitsee, että miekka ei ole enää heilautuksessa.
        isSwinging = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tarkistaa, osuuko miekka viholliseen ja onko varustettu esine olemassa.
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Sword hit enemy {other.name}");

            // Hakee vihollisen EnemyStats-komponentin.
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                // Hakee aktiivisesti valitun esineen InventoryManagerista.
                Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

                // Vähentää vihollisen terveyttä miekan vahingon määrällä.
                if (equippedItem != null && equippedItem.isWeapon)
                {
                    int damage = equippedItem.attackDamage;
                    enemyStats.maxHealth = Mathf.Max(0, enemyStats.maxHealth - damage);
                    Debug.Log($"Hit enemy! Damage: {damage}, Remaining health: {enemyStats.maxHealth}");

                    // Tarkistaa, kuoleeko vihollinen, ja kutsuu tarvittaessa sen kuoleman käsittelevän metodin.
                    if (enemyStats.maxHealth == 0)
                    {
                        other.GetComponent<EnemyController>()?.Death();
                    }
                }
                else
                {
                    Debug.LogWarning("No weapon equipped or item is not a weapon!");
                }
            }
            else
            {
                Debug.LogWarning("Enemy does not have an EnemyStats component.");
            }
        }
    }
}
