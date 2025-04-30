using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeholderAnimation : MonoBehaviour
{
    [Header("References (do not touch)")]
    [SerializeField] private BossBase bossBase;
    [SerializeField] private Animator animator;
    [SerializeField] public Transform laserHolder;
    public Laser laser;

    [Header("Tunable Attack Settings")]
    public bool facingUp = false;
    [SerializeField] public bool useLaserOffsets;
    private int currentAttack;

    [Header("Laser?Holder Offsets (8 slots)")]
    [SerializeField]
    public Vector2[] laserOffsets = new Vector2[8] {
        new Vector2(-0.006f, -0.1034f), // Down

        new Vector2( 0.093f, -0.112f),
        new Vector2( 0.158f, -0.059f), // Right
        new Vector2( 0.114f, -0.059f),

        new Vector2(-0.006f, -0.059f), // Up
        
        new Vector2( -0.114f, -0.059f),
        new Vector2( -0.158f, -0.059f), // Left
        new Vector2( -0.093f, -0.112f)
    };

    [Header("Bounce Attack Settings")]
    [SerializeField] private float bounceSpeed = 5f;
    [SerializeField] private float minBounceAngle = 10f;
    [SerializeField] private float maxBounceAngle = 30f;
    [SerializeField] private float minBounceTime = 2f;
    [SerializeField] private float maxBounceTime = 5f;

    private float bounceTimer;
    private Vector2 bounceDir;

    private bool isSpinning = false;
    private bool isBouncing = false;
    private bool isAttackInProgress = false;

    private int lastOffsetIndex = -1;
    private int bossSortingOrder;
    private Rigidbody2D rb;
    private LayerMask hitMask;

    private void Awake()
    {
        // Physics?driven bounces
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        bossSortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        laserHolder.gameObject.SetActive(true);

        // grab hitMask from laser
        hitMask = laser.HitMask;
    }

    private void FixedUpdate()
    {
        if (isBouncing)
        {
            // countdown bounce duration
            bounceTimer -= Time.deltaTime;
            if (bounceTimer <= 0f)
                EndBounce();
            return;
        }
        if (isSpinning)
            RotateAndAnimateLaser();
    }

    public void SetHitMask(LayerMask mask)
    {
        hitMask = mask;
    }

    public void SetCurrentAttack(int attack)
    {
        currentAttack = attack;
        animator.SetInteger("CurrentAttack", attack);
    }

    public void TriggerAttack(bool facingUpDirection)
    {
        facingUp = facingUpDirection;
        isAttackInProgress = true;

        animator.SetBool("LookingUp", facingUp);
        animator.SetBool("Attacking", true);
        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", facingUp ? 1f : -1f);

        bossBase.SetAttackState(true);
    }

    public void TriggerBounceAttack(bool startBounceRight)
    {
        isAttackInProgress = true;
        isBouncing = true;

        // randomize bounce angle and duration
        float angleDeg = Random.Range(minBounceAngle, maxBounceAngle);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        bounceTimer = Random.Range(minBounceTime, maxBounceTime);

        float dx = startBounceRight ? 1f : -1f;
        float dy = Mathf.Tan(angleRad) * Mathf.Sign(angleRad);
        bounceDir = new Vector2(dx, dy).normalized;

        // apply initial velocity
        rb.linearVelocity = bounceDir * bounceSpeed;

        animator.SetBool("Attacking", true);
        bossBase.SetAttackState(true);
    }

    public void OnAttackPrepEnd()
    {
        // only laser-spin if we're in the laser attack (attackInt == 1)
        if (currentAttack != 1 || isBouncing)
            return;

        // choose spin direction at random
        bool spinCW = (Random.value > 0.5f);
        bossBase.SetupSpin(clockwise: spinCW, facingUp);

        laser.FireLaser();
        isSpinning = true;
    }

    public void OnAttackEnd()
    {
        isSpinning = false;
        laser.EndLaser();

        isAttackInProgress = false;
        animator.SetBool("Attacking", false);
        bossBase.SetAttackState(false);
    }

    public bool IsAttackInProgress() => isAttackInProgress;

    public void FollowTarget(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        transform.Translate(dir * Time.deltaTime * bossBase.MoveSpeed, Space.World);
        animator.SetFloat("Horizontal", dir.x);
        animator.SetFloat("Vertical", dir.y);
        animator.SetFloat("Speed", 1f);
        bossBase.FaceDirection(dir);
    }

    public void Death()
    {
        bossBase.SetAttackState(false);
        animator.CrossFade("death", 0.1f);
        enabled = false;
    }

    private void RotateAndAnimateLaser()
    {
        bossBase.RotateAttackDirection();
        float angle = bossBase.GetCurrentLaserAngle() + 90f;
        laserHolder.rotation = Quaternion.Euler(0f, 0f, angle);

        if (!bossBase.IsRotating)
        {
            isSpinning = false;
            animator.SetBool("Attacking", false);
            laser.EndLaser();
            return;
        }

        float sliced = (bossBase.GetCurrentLaserAngle() + 360f) % 360f;
        sliced = (sliced + 112.5f) % 360f;
        int index = Mathf.FloorToInt(sliced / 45f);

        if (index != lastOffsetIndex)
        {
            if (useLaserOffsets)
            {
                Vector2 offset = laserOffsets[index % laserOffsets.Length];
                if (bossBase.SpriteRenderer.flipX)
                    offset.x *= -1f;
                Vector3 z = Vector3.forward * laserHolder.localPosition.z;
                laserHolder.localPosition = (Vector3)offset + z;
            }
            bool behind = (index == 3 || index == 4 || index == 5);
            laser.SetSortingOrder(behind ? bossSortingOrder - 1 : bossSortingOrder + 1);
            lastOffsetIndex = index;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBouncing) return;
        if (((1 << collision.gameObject.layer) & hitMask) == 0) return;
    }

    private void EndBounce()
    {
        isBouncing = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Attacking", false);
        isAttackInProgress = false;
        bossBase.SetAttackState(false);
    }
}