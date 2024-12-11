using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Viittaus miekan GameObjectiin, jota k�ytet��n ly�ntianimaatioihin.
    public GameObject sword;

    // Nopeus, jolla miekkaa heilautetaan (asteet sekunnissa).
    public float swingSpeed = 200f;

    // Tieto siit�, onko miekka parhaillaan heilautuksessa.
    private bool isSwinging = false;

    // Ly�nnin suunta (oletuksena oikealle).
    private Vector2 attackDirection = Vector2.right;

    // M��riteltyj� Transform-pisteit�, joihin miekka siirtyy eri hy�kk�yssuunnissa.
    [SerializeField] private Transform UpSwing;
    [SerializeField] private Transform DownSwing;
    [SerializeField] private Transform LeftSwing;
    [SerializeField] private Transform RightSwing;

    // Miekan liikkeen asetukset, jotka voidaan s��t�� Unityn Inspectorissa.
    [Header("Swing Settings")]
    [Tooltip("Miekan liikkuma et�isyys (yksik�it�) heilautuksen aikana.")]
    public float swingDistance = 2.0f;

    [Tooltip("Aika (sekunteina), joka menee heilautukseen eteenp�in ja takaisin.")]
    public float swingDuration = 0.5f;

    // Miekan heilautuksen kulkusuunta.
    private Vector3 swingEndPosition;

    void Start()
    {
        // Tarkistetaan, ett� miekka on m��ritetty, ja annetaan varoitus, jos ei ole.
        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned to PlayerCombat!");
        }
    }

    void Update()
    {
        // P�ivitt�� hy�kk�yssuunnan nuolin�pp�imien perusteella.
        HandleAttackDirection();

        // Aloittaa miekan heilautuksen, jos v�lily�nti� painetaan eik� miekka ole jo heilautuksessa.
        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartCoroutine(SwingSword());
        }
    }

    void HandleAttackDirection()
    {
        // P�ivitt�� hy�kk�yssuunnan nuolin�pp�imien perusteella.
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

        // P�ivitt�� miekan sijainnin hy�kk�yssuunnan mukaan.
        UpdateSwordPosition();
    }

    void UpdateSwordPosition()
    {
        // Siirt�� miekan sijainnin oikeaan kohtaan m��ritettyjen Transform-pisteiden perusteella.
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
        // Merkitsee, ett� miekka on heilautuksessa.
        isSwinging = true;

        // Miekan alkuper�inen sijainti.
        Vector3 initialPosition = sword.transform.position;

        // M��ritet��n, mihin suuntaan miekka liikkuu.
        swingEndPosition = initialPosition + (Vector3)attackDirection * swingDistance;

        // Liikkeen nopeus ja aikaraja.
        float elapsedTime = 0f;

        // Liikutetaan miekkaa eteenp�in.
        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(initialPosition, swingEndPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Varmistetaan, ett� miekka on p��ssyt t�yteen et�isyyteens�.
        sword.transform.position = swingEndPosition;

        // Palautetaan miekka alkuper�iseen paikkaansa.
        elapsedTime = 0f;
        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(swingEndPosition, initialPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asetetaan miekan sijainti alkuper�iseen tilaan.
        sword.transform.position = initialPosition;

        // Merkitsee, ett� miekka ei ole en�� heilautuksessa.
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

                // V�hent�� vihollisen terveytt� miekan vahingon m��r�ll�.
                if (equippedItem != null && equippedItem.isWeapon)
                {
                    int damage = equippedItem.attackDamage;
                    enemyStats.maxHealth = Mathf.Max(0, enemyStats.maxHealth - damage);
                    Debug.Log($"Hit enemy! Damage: {damage}, Remaining health: {enemyStats.maxHealth}");

                    // Tarkistaa, kuoleeko vihollinen, ja kutsuu tarvittaessa sen kuoleman k�sittelev�n metodin.
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
