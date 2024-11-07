using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int attackDamage = 25;         // Vahingon määrä
    public float attackRange = 1.5f;      // Hyökkäyksen kantama
    public LayerMask enemyLayer;          // Maski, joka määrittää "viholliset" (esim. layer vihollisille)

    public Transform attackPoint;         // Pelaajan edessä oleva piste, josta hyökkäys lähtee

    void Update()
    {
        // Tarkistetaan, onko pelaaja painanut hyökkäysnäppäintä (oletetaan "Fire1" eli vasen hiiren nappi)
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    void Attack()
    {
        // Luodaan ympyrän muotoinen alue hyökkäyspisteen ympärille
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Käydään läpi kaikki osuneet viholliset
        foreach (Collider2D enemy in hitEnemies)
        {
            // Annetaan vahinkoa vihollisen Health-komponentin kautta
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }

    // Piirretään hyökkäyksen kantama editoriin apuviivana
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
