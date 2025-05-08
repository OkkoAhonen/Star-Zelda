using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class CrystalProjectile : MonoBehaviour
{
    [Tooltip("Damage dealt if the player touches it")]
    [SerializeField] private int damage = 10;
    [Tooltip("How long collider stays active after landing")]
    [SerializeField] private float lifeAfterLanding = 0.5f;
    [Tooltip("Health of the crystal; breaks if ? 0")]
    [SerializeField] private float crystalHealth = 15f;

    private CircleCollider2D col;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.enabled = false;
    }

    // Called by your “Land” animation event
    public void EnableCollider()
    {
        col.enabled = true;
        StartCoroutine(DestroyAfter(lifeAfterLanding));
    }

    private IEnumerator DestroyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Called by your “Break” animation event or when health ? 0
    public void BreakCrystal()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float amt)
    {
        crystalHealth -= amt;
        if (crystalHealth <= 0f) BreakCrystal();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            PlayerStatsManager.instance.TakeDamage(damage);
        }
    }
}