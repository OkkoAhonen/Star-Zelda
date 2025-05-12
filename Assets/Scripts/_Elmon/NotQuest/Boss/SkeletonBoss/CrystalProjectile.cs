using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class CrystalProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeAfterLanding = 0.5f;
    private float crystalHealth = 15f;

    public void SetHealth(float h) { crystalHealth = h; }
    public void StartLifespan() { StartCoroutine(Lifespan()); }

    private IEnumerator Lifespan()
    {
        yield return new WaitForSeconds(lifeAfterLanding);
        Destroy(gameObject);
    }

    public void BreakCrystal() { Destroy(gameObject); }

    public void TakeDamage(float amt)
    {
        crystalHealth -= amt;
        if (crystalHealth <= 0f) BreakCrystal();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
            PlayerStatsManager.instance.TakeDamage(damage);
    }
}