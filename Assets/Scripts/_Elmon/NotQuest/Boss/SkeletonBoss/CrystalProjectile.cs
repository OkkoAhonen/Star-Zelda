using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class CrystalProjectile : MonoBehaviour
{
    [Tooltip("Damage dealt to the player")]
    [SerializeField] private int damage = 10;
    [Tooltip("Time until the crystal hits the ground (seconds)")]
    [SerializeField] private float groundDelay = 1f;
    [Tooltip("Time collider remains active after landing")]
    [SerializeField] private float lifeAfterLanding = 0.5f;

    private CircleCollider2D col2d;

    void Awake()
    {
        col2d = GetComponent<CircleCollider2D>();
        col2d.enabled = false;
    }

    void Start()
    {
        StartCoroutine(Lifecycle());
    }

    private IEnumerator Lifecycle()
    {
        // fall from roof
        yield return new WaitForSeconds(groundDelay);

        // enable damage collider
        col2d.enabled = true;

        // wait a bit then disappear
        yield return new WaitForSeconds(lifeAfterLanding);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatsManager.instance.TakeDamage(damage);
        }
    }
}