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
    private float rushSpeedMultiplier = 2f; // Rynn�kk�nopeus
    private float attackCooldownTimer = 0f;

    private float attackCooldown = 3f; // Aika, jonka bossilla menee hy�kk�yksen v�lill�

    public GameObject projectilePrefab; // Projektiili, joka ammutaan pelaajaa kohti

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Etsit��n pelaaja
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
        if (enemyStats.maxHealth <= 0) return; // Jos pomo on kuollut, ei tehd� mit��n

        attackCooldownTimer -= Time.deltaTime; // Aika, jonka pomo voi odottaa ennen seuraavaa hy�kk�yst�

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
        // Pomo odottaa ja p��tt��, mit� tehd� seuraavaksi
        if (attackCooldownTimer <= 0f)
        {
            int action = Random.Range(0, 2); // Arvotaan satunnaisesti, l�hteek� rynnist�m��n vai lataamaan projektiili�

            if (action == 0)
            {
                currentState = BossState.Rushing; // Vaihdetaan rynn�kk�tilaan
            }
            else
            {
                currentState = BossState.Charging; // Vaihdetaan lataustilaan
            }

            attackCooldownTimer = attackCooldown; // Asetetaan hy�kk�yksen odotusaika
        }
    }

    void RushingBehavior()
    {
        // Boss rynt�� pelaajaa kohti
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * enemyStats.speed * rushSpeedMultiplier; // Rynn�kk�nopeus

        if (Vector2.Distance(transform.position, player.position) < 1f)
        {
            // Pomo t�rm�� pelaajaan
            // Voit lis�t� my�s vahingon pelaajalle t�ss� kohtaa, jos haluat
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
        // Lis�� erikoishy�kk�yksi� t�nne, jos haluat monimutkaisempaa k�yt�st�
        currentState = BossState.Idle; // Palataan takaisin odottavaan tilaan
    }

    void FireProjectile()
    {
        // Projektiilin ammunta
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f; // Ammutaan pelaajaa kohti
        Destroy(projectile, 5f); // Tuhoa projektiili, jos se ei osu pelaajaan tietyn ajan j�lkeen
    }
}
