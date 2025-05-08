using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float speed;
    private Vector2 moveDirection;

    [Header("Settings")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damageAmount = 10;
    // [SerializeField] private string playerTag = "Player";

    // [SerializeField] private GameObject hitEffectPrefab;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ProjectileController: Rigidbody2D not found on this projectile!", gameObject);
        }
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, float projectileSpeed)
    {
        this.moveDirection = direction.normalized;
        this.speed = projectileSpeed;
        if (rb != null)
        {
            rb.linearVelocity = this.moveDirection * this.speed;
        }
    }

}