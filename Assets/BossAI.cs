using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Charging, Attacking, Rushing }
    private BossState currentState = BossState.Idle;

    private Transform player;
    private EnemyStats enemyStats;
    private Rigidbody2D rb;
    private Vector2 targetPosition;

    private float chargeTime = 2f; // Aika, jonka boss lataa projektiilin
    private float rushSpeedMultiplier = 2f; // Rynnäkkönopeus
    private float attackCooldownTimer = 0f;

    private float attackCooldown = 3f; // Aika, jonka bossilla menee hyökkäyksen välillä

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
        if (enemyStats.maxHealth <= 0) return; // Jos pomo on kuollut, ei tehdä mitään

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

    void RushingBehavior()
    {
        // Boss ryntää pelaajaa kohti
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * enemyStats.speed * rushSpeedMultiplier; // Rynnäkkönopeus

        if (Vector2.Distance(transform.position, player.position) < 1f)
        {
            // Pomo törmää pelaajaan
            // Voit lisätä myös vahingon pelaajalle tässä kohtaa, jos haluat
            currentState = BossState.Idle; // Palataan takaisin odottavaan tilaan
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
}
