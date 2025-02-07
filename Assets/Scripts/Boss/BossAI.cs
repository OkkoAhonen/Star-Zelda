using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Charging, Attacking, Rushing }
    [SerializeField] private BossState currentState = BossState.Idle;

    [SerializeField] private Transform player;
    private EnemyStats enemyStats;
    private Rigidbody2D rb;
    private Vector2 targetPosition;

    public SceneManager sceneManager;
    public Transform shootingpoint;

    private float chargeTime = 1.5f; // Aika, jonka boss lataa projektiilin
    private float rushSpeedMultiplier = 2f; // Rynnäkkönopeus
    private float attackCooldownTimer = 0f;

    private float attackCooldown = 1f; // Aika, jonka bossilla menee hyökkäyksen välillä

    public GameObject projectilePrefab; // Projektiili, joka ammutaan pelaajaa kohti

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Etsitään pelaaja
        enemyStats = GetComponent<EnemyStats>(); // Saadaan viittaus EnemyStats-komponenttiin
        rb = GetComponent<Rigidbody2D>(); // Saadaan Rigidbody2D, jotta voimme liikkua

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }

        if (enemyStats == null)
        {
            Debug.LogError("EnemyStats component missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyStats.maxHealth <= 0) {
            Destroy(gameObject);
            SceneManager.LoadScene("Town kokeilu");
            
            return; } // Jos pomo on kuollut, ei tehdä mitään

        attackCooldownTimer -= Time.deltaTime; // Aika, jonka pomo voi odottaa ennen seuraavaa hyökkäystä

        switch (currentState)
        {
            case BossState.Idle:
                IdleBehavior();
                break;

            case BossState.Rushing:
                RushingBehavior();
                break;

            case BossState.Attacking:
                AttackingBehavior();
                break;

            case BossState.Charging:
                ChargingBehavior();
                break;
        }
    }

    void IdleBehavior()
    {
        // Pomo odottaa ja päättää, mitä tehdä seuraavaksi
        if (attackCooldownTimer <= 0f)
        {
            int action = Random.Range(0, 2); // Arvotaan satunnaisesti, lähteekö rynnistämään vai lataamaan projektiiliä

            if (action == 0)
            {
                currentState = BossState.Rushing; // Vaihdetaan rynnäkkötilaan
            }
            else
            {
                currentState = BossState.Charging; // Vaihdetaan lataustilaan
            }

            attackCooldownTimer = attackCooldown; // Asetetaan hyökkäyksen odotusaika
        }
    }

    private float stateTimer = 0f; // Seuraa tilan kestoa

    void RushingBehavior()
    {
        // Päivitä ajastin
        stateTimer += Time.deltaTime;

        // Tarkista, kauanko tila on jatkunut
        int timer = Random.Range(3, 12);

        if (stateTimer > timer)
        {
            Debug.Log("Rushing too long, switching to Charging state.");
            currentState = BossState.Charging; // Vaihdetaan lataustilaan
            stateTimer = 0f; // Nollataan ajastin seuraavaa tilaa varten
            return;
        }

        // Boss ryntää pelaajaa kohti
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * enemyStats.speed * rushSpeedMultiplier; // Rynnäkkönopeus

        if (Vector2.Distance(transform.position, player.position) < 1f)
        {
            // Pomo törmää pelaajaan
            // Voit lisätä myös vahingon pelaajalle tässä kohtaa, jos haluat
            currentState = BossState.Idle; // Palataan takaisin odottavaan tilaan
            stateTimer = 0f; // Nollataan ajastin seuraavaa tilaa varten
        }
    }

    void ChargingBehavior()
    {
        // Boss valmistautuu ampumaan projektiilin pelaajaa kohti
        if (attackCooldownTimer <= 0f)
        {
            FireProjectile(); // Ammutaan projektiili
            currentState = BossState.Idle; // Palataan takaisin odottavaan tilaan
        }
    }

    void AttackingBehavior()
    {
        // Lisää erikoishyökkäyksiä tänne, jos haluat monimutkaisempaa käytöstä
        currentState = BossState.Idle; // Palataan takaisin odottavaan tilaan
    }

    void FireProjectile()
    {
        // Projektiilin ammunta
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f; // Ammutaan pelaajaa kohti
        Destroy(projectile, 5f); // Tuhoa projektiili, jos se ei osu pelaajaan tietyn ajan jälkeen
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Pelaajalle vahinkoa vihollisen hyökkäysvoimalla
            PlayerMovement2D playerMovement = collision.gameObject.GetComponent<PlayerMovement2D>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(enemyStats.attackDmg);
            }
        }
    }
}