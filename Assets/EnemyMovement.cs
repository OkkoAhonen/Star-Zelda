using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player; // Pelaajan sijainti
    public float speed = 2f; // Liikkumisnopeus
    public float detectionRange = 5f; // Havaintoet‰isyys
    public float avoidanceDistance = 1f; // Kuinka kaukaa esteit‰ kierret‰‰n
    public LayerMask obstacleLayer; // Esteiden layer

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 moveDirection = direction;

            // Tarkista esteet raycastilla
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, avoidanceDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // Jos este osuu, kierr‰ sit‰
                Vector2 avoidanceDir = Vector2.Perpendicular(direction).normalized;
                moveDirection = avoidanceDir; // Tai -avoidanceDir, jos haluat toiseen suuntaan
            }

            // Liiku
            rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
