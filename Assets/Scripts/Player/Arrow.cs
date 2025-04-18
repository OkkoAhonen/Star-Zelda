using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private float speed;      // Arrow movement speed
    [SerializeField] private float damage;     // Damage it deals
    [SerializeField] private float lifespan;   // Max lifetime
    [SerializeField] private float lifeTimer;  // Tracks time alive

    public float Speed => speed;
    public float Damage => damage;
    public float Lifespan => lifespan;
    public float LifeTimer => lifeTimer;

    public void Initialize(float newSpeed, float newDamage, float newLifespan)
    {
        speed = newSpeed;
        damage = newDamage;
        lifespan = newLifespan;
        lifeTimer = 0f;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifespan)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<EnemyAI>()?.enemyTakeDamage(damage);

        Destroy(gameObject);
    }
}
