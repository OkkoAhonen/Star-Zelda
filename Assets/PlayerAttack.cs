using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int attackDamage = 25;         // Vahingon m��r�
    public float attackRange = 1.5f;      // Hy�kk�yksen kantama
    public LayerMask enemyLayer;          // Maski, joka m��ritt�� "viholliset" (esim. layer vihollisille)

    public Transform attackPoint;         // Pelaajan edess� oleva piste, josta hy�kk�ys l�htee

    void Update()
    {
        // Tarkistetaan, onko pelaaja painanut hy�kk�ysn�pp�int� (oletetaan "Fire1" eli vasen hiiren nappi)
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    void Attack()
    {
        // Luodaan ympyr�n muotoinen alue hy�kk�yspisteen ymp�rille
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // K�yd��n l�pi kaikki osuneet viholliset
        foreach (Collider2D enemy in hitEnemies)
        {
            // Annetaan vahinkoa vihollisen Health-komponentin kautta
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }

    // Piirret��n hy�kk�yksen kantama editoriin apuviivana
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
