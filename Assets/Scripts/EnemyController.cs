using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Wander,
    Follow,
    Die
}

public class EnemyController : MonoBehaviour
{
    GameObject player;
    public EnemyState currentState = EnemyState.Wander;
    public EnemyStats enemyStats;

    public float range;
    public float speed;
    private bool chooseDir = false;
    private bool dead = false;
    private Vector3 randomDir;

    // Start is called before the first frame update
    void Start()
    {
        range = 100f;
        player = GameObject.FindGameObjectWithTag("Player");

        if (enemyStats == null)
        {
            Debug.LogError("EnemyStats is not assigned for " + gameObject.name);
            return;
        }

        // Asetetaan nopeus EnemyStats-komponentista.
        speed = enemyStats.speed;
    }

    // Update is called once per frame
    void Update()
    {
        speed = enemyStats.speed;
        if (dead)
            return;

        switch (currentState)
        {
            case EnemyState.Wander:
                Wander();
                break;

            case EnemyState.Follow:
                Follow();
                break;

            case EnemyState.Die:
                break;
        }

        if (IsPlayerInRange(range) && currentState != EnemyState.Die)
        {
            currentState = EnemyState.Follow;
        }
        else if (!IsPlayerInRange(range) && currentState != EnemyState.Die)
        {
            currentState = EnemyState.Wander;
        }
    }

    private bool IsPlayerInRange(float range)
    {
        if (player == null)
            return false;

        return Vector3.Distance(transform.position, player.transform.position) <= range;
    }

    private IEnumerator ChooseDirection()
    {
        chooseDir = true;
        yield return new WaitForSeconds(Random.Range(2f, 8f));

        // Satunnainen suunta viholliselle.
        randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        chooseDir = false;
    }

    void Wander()
    {
        if (!chooseDir)
        {
            StartCoroutine(ChooseDirection());
        }

        // Liikkuu satunnaiseen suuntaan.
        transform.position += randomDir * speed * Time.deltaTime;

        if (IsPlayerInRange(range))
        {
            currentState = EnemyState.Follow;
        }
    }

    void Follow()
    {
        if (player == null)
            return;

        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    public void Death()
    {
        dead = true;
        currentState = EnemyState.Die;
        Destroy(gameObject);
    }
}
