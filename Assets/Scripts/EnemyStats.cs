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
        // Muutetaan nimi pieniksi kirjaimiksi.
        string lowerName = name.ToLower();

        // Tarkistetaan, sisältääkö nimi "zombie".
        if (lowerName.Contains("zombie"))
        {
            maxHealth = 15f;
            attackDmg = 1.5f;
            speed = 3.5f;
            cooldown = 1;
        }
        // Tarkistetaan, sisältääkö nimi "skeleton".
        else if (lowerName.Contains("skeleton"))
        {
            maxHealth = 30f;
            attackDmg = 3f;
            speed = 5f;
            cooldown = 1;
        }
        // Tarkistetaan, sisältääkö nimi "slime".
        else if (lowerName.Contains("slime"))
        {
            maxHealth = 20f;
            attackDmg = 2f;
            speed = 2f;
            cooldown = 2;
        }
        // Tarkistetaan, sisältääkö nimi "void".
        else if (lowerName.Contains("void"))
        {
            maxHealth = 50f;
            attackDmg = 6f;
            speed = 5f;
            cooldown = 2;
        }
        // Tarkistetaan, sisältääkö nimi "boss".
        else if (lowerName.Contains("boss"))
        {
            maxHealth = 100f;
            attackDmg = 10f;
            speed = 1.5f;
            cooldown = 3;
        }
        else
        {
            Debug.LogWarning("Enemy type not recognized: " + name);
            SetDefaultStats(); // Käytetään oletusarvoja.
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
