using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public GameObject enemy; // T‰m‰ tulee m‰‰ritt‰‰ Unity-editorissa.

    public float maxHealth;
    public float attackDmg;
    public float speed;
    public int cooldown;
    public string enemyName; // Korjattu "name" ristiriitojen v‰ltt‰miseksi.

    // Start is called before the first frame update
    void Start()
    {
        if (enemy != null) // Tarkistetaan, ett‰ enemy on asetettu.
        {
            enemyName = enemy.name;
            SetEnemyStats(enemyName);
        }
        else
        {
            Debug.LogError("Enemy GameObject is not assigned in the Inspector for " + gameObject.name);
        }
    }

    void SetEnemyStats(string name)
    {
        switch (name.ToLower()) // K‰ytet‰‰n ToLower() varmistamaan, ettei kirjainkoolla ole v‰li‰.
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
                maxHealth = 10f; // Oletusarvot tuntemattomille vihollisille.
                attackDmg = 1f;
                speed = 1f;
                cooldown = 1;
                break;
        }
    }
}
