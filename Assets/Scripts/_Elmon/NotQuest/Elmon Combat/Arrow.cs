using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Populated by Bow on Instantiate
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public float arrowLifeSpan;

    private Rigidbody2D _rb;
    private float _lifeTimer;
    private bool _stuck;

    // LayerMasks from your static manager
    private LayerMask hitMask;
    private LayerMask damageMask;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Ensure it uses linearVelocity and no gravity
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;

        // Grab your masks
        hitMask = StaticValueManager.HitMask;
        damageMask = StaticValueManager.DamageEnemiesMask;
    }

    private void Start()
    {
        // Launch along local +X
        _rb.linearVelocity = transform.right * speed;
        _lifeTimer = arrowLifeSpan;
    }

    private void Update()
    {
        if (_stuck) return;

        // Auto?destroy after lifespan
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_stuck) return;

        int layerBit = 1 << other.gameObject.layer;

        // Hit wall/terrain ? stick
        if ((layerBit & hitMask) != 0)
        {
            StickToTarget(other.transform);
        }
        // Hit enemy ? damage & destroy
        else if ((layerBit & damageMask) != 0)
        {
            DamageEnemy(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void StickToTarget(Transform target)
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        transform.SetParent(target, true);
        _stuck = true;
    }

    private void DamageEnemy(GameObject enemy)
    {
        Debug.LogError("Player arrow hit: " + enemy.name);
        // TODO: integrate your real damage system here, using `damage`
    }
}