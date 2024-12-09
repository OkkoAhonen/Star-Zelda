using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public GameObject enemy;

    public float maxHealth;
    public float attackDmg;
    public float speed;
    public int cooldown;
    public string name;


    // Start is called before the first frame update
    void Start()
    {
        GameObject enemy = GetComponent<GameObject>();
        name = enemy.name;

        Enemy(name, maxHealth, attackDmg, speed, cooldown);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Enemy(string name, float health, float dmg, float speed, int cooldown)
    {
        switch (name)
        {
            case "skeleton":
                health = 30f;
                dmg = 3f;
                speed = 5f;
                cooldown = 1;
                break;

            case "zombie":
                health = 15f;
                dmg = 1.5f;
                speed = 3.5f;
                cooldown = 1;
                break;

            case "slime":
                health = 20f;
                dmg = 2f;
                speed = 2f;
                cooldown = 2;
                break;

            case "void":
                health = 50f;
                dmg = 6f;
                speed = 5f;
                cooldown = 2;
                break;

            case "boss":
                health = 100f;
                dmg = 10f;
                speed = 5f;
                cooldown = 3;
                break;

            default:
                break;
        }
    }
}
