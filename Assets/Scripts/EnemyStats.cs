using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public GameObject enemy; // Asetetaan Unity-editorissa.

    public float maxHealth;
    public float attackDmg;
    public float speed;
    public int cooldown;
    public string enemyName; // Korjattu ristiriitojen välttämiseksi.

    // Start is called before the first frame update
    void Start()
    {
        if (enemy != null) // Tarkistetaan, että enemy on asetettu.
        {
            enemyName = enemy.name;
            SetEnemyStats(enemyName);
        }
        else
        {
            Debug.LogError("Enemy GameObject is not assigned in the Inspector for " + gameObject.name);
            // Varmistetaan, että ominaisuuksilla on oletusarvot.
            SetDefaultStats();
        }

        // Varmistetaan, ettei maxHealth ole nolla tai negatiivinen.
        if (maxHealth <= 0)
        {
            Debug.LogWarning("Enemy " + gameObject.name + " has invalid maxHealth. Setting default value.");
            maxHealth = 10f; // Oletusarvo.
        }
    }

    void SetEnemyStats(string name)
    {
        switch (name.ToLower()) // Kirjainkoon varmistus.
        {
            case "skeleton":
                maxHealth = 30f;
                attackDmg = 3f;
                speed = 5f;
                cooldown = 1;
                break;

            case "zombie":
                maxHealth = 15f;
                attackDmg = 1.5f;
                speed = 3.5f;
                cooldown = 1;
                break;

            case "slime":
                maxHealth = 20f;
                attackDmg = 2f;
                speed = 2f;
                cooldown = 2;
                break;

            case "void":
                maxHealth = 50f;
                attackDmg = 6f;
                speed = 5f;
                cooldown = 2;
                break;

            case "boss":
                maxHealth = 100f;
                attackDmg = 10f;
                speed = 5f;
                cooldown = 3;
                break;

            default:
                Debug.LogWarning("Enemy type not recognized: " + name);
                SetDefaultStats(); // Käytetään oletusarvoja.
                break;
        }
    }

    void SetDefaultStats()
    {
        maxHealth = 10f; // Oletusarvot.
        attackDmg = 1f;
        speed = 1f;
        cooldown = 1;
    }
}
