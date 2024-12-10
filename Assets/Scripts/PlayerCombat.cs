using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int hitDamage = 3; // Miekan tekemä vahinko.
    public GameObject sword; // Pelaajan miekka.

    // Start is called before the first frame update
    void Start()
    {
        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned to PlayerCombat!");
        }
    }

    // Tämä metodi kutsutaan, kun miekka osuu viholliseen.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("lyönti");

        // Tarkistetaan, osuuko miekka viholliseen.
        //if (other.CompareTag("Enemy"))
        {
            Debug.Log("lyönti viholliseen " + other.name);
            // Haetaan EnemyStats-komponentti viholliselta.
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                // Vähennetään vihollisen terveyttä.
                enemyStats.maxHealth -= hitDamage;
                Debug.Log("Hit enemy! Remaining health: " + enemyStats.maxHealth);

                // Tarkistetaan, kuoleeko vihollinen.
                if (enemyStats.maxHealth <= 0)
                {
                    enemyStats.maxHealth = 0;
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
