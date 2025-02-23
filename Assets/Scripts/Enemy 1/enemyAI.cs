using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Enemy enemystats; // Muista asettaa tämä Inspectorissa!
    private Rigidbody2D rb;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject lootprefab;
    [SerializeField] private Item dropItem;
    public float health;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        health = enemystats.maxHealth;

        if (player == null)
        {
            Debug.LogError("Pelaajaa ei löytynyt! Varmista, että GameObjectilla on tagi 'Player'.");
        }

        if (enemystats == null)
        {
            Debug.LogError("Enemystats ei ole asetettu! Vedä EnemyStats-objekti EnemyAI-komponenttiin.");
        }
    }

    void Update()
    {
        WatchHealth();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            playerAction playerAction = collision.gameObject.GetComponent<playerAction>();
            playerAction.playerTakeDamage(enemystats.strength);
        }
    }

    void Die()
    {
        GameObject lootObject = Instantiate(lootprefab, transform.position, transform.rotation);
        Loot loot = lootObject.GetComponent<Loot>();
        if (loot != null && dropItem != null)
        {
            loot.Initialize(dropItem);
        }
        Destroy(gameObject);
    }

    public void enemyTakeDamage(float damage)
    {
        health -= damage;
    }

    void WatchHealth()
    {
        if (health <= 0)
        {
            Die();
        }
    }
}