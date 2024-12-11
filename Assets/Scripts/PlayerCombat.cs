using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Item equippedItem; // Pelaajan varustettu miekka (ScriptableObject)
    public GameObject sword; // Miekan fyysinen GameObject
    public float swingSpeed = 200f; // Miekan heilautuksen nopeus (asteet per sekunti).
    private bool isSwinging = false; // Onko miekka parhaillaan heilautuksessa.
    private Vector2 attackDirection = Vector2.right; // Lyönnin oletussuunnan vektori.

    [SerializeField] private Transform UpSwing;
    [SerializeField] private Transform DownSwing;
    [SerializeField] private Transform LeftSwing;
    [SerializeField] private Transform RightSwing;

    void Start()
    {
        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned to PlayerCombat!");
        }

        if (equippedItem == null)
        {
            Debug.LogError("No item equipped! Assign an Item in the Inspector.");
        }
    }

    void Update()
    {
        HandleAttackDirection(); // Päivittää lyönnin suunnan nuolinäppäimistä.

        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartCoroutine(SwingSword());
        }
    }

    void HandleAttackDirection()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            attackDirection = Vector2.up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            attackDirection = Vector2.down;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            attackDirection = Vector2.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            attackDirection = Vector2.right;
        }

        UpdateSwordPosition();
    }

    void UpdateSwordPosition()
    {
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
        isSwinging = true;

        // Lasketaan perussuunnan kulma hyökkäyssuunnan perusteella.
        float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - 45f; // Aloita vasemmalta.
        float endAngle = baseAngle + 45f;  // Lopeta oikealle.
        float currentAngle = startAngle;

        while (currentAngle <= endAngle)
        {
            sword.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            currentAngle += swingSpeed * Time.deltaTime;

            yield return null;
        }

        sword.transform.localRotation = Quaternion.Euler(0, 0, 0);
        isSwinging = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && equippedItem != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);


            Debug.Log($"Sword hit enemy {other.name}");

            // Haetaan EnemyStats-komponentti viholliselta.
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                // Vähennetään vihollisen terveyttä käyttäen varustetun miekan vahinkoa.
                int damage = equippedItem.attackDamage; // Miekan vahinko ScriptableObjectista
                enemyStats.maxHealth = Mathf.Max(0, enemyStats.maxHealth - damage);
                Debug.Log($"Hit enemy! Damage: {damage}, Remaining health: {enemyStats.maxHealth}");

                // Tarkistetaan, kuoleeko vihollinen.
                if (enemyStats.maxHealth == 0)
                {
                    other.GetComponent<EnemyController>()?.Death();
                }
            }
            else
            {
                Debug.LogWarning("Enemy does not have an EnemyStats component.");
            }
        }
    }
}
