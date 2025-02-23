using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Enemy enemystats; // Muista asettaa tämä Inspectorissa!
    private Rigidbody2D rb;
    [SerializeField] private GameObject player;

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

        //Movement towards player
        /*if (player != null && enemystats != null)
        {
            //Vector2 newPositon = Vector2.MoveTowards(rb.position, player.transform.position, enemystats.speed * Time.fixedDeltaTime);
            //rb.MovePosition(newPositon);
        }*/
    }


    //Damage player on collisions
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
        Destroy(gameObject);
    }

    public void enemyTakeDamage(float damage)
    {
        health -= damage;
    }

    void WatchHealth()
    {
        if(health <= 0)
        {
            Die();
        }
    }
}
