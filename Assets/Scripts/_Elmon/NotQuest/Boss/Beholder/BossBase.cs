using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Movement (tweak if you like)")]
    public float moveSpeed = 5f;

    [Header("Tunable Spin Settings")]
    [Tooltip("Minimum half-spins (180° per half-spin)")]
    public int minHalfSpins = 1;
    [Tooltip("Maximum half-spins (180° per half-spin)")]
    public int maxHalfSpins = 2;
    public float spinSpeed = 90f;

    [Header("References (do not touch)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isAttacking;
    public bool IsRotating { get; private set; }
    private float currentAngle;
    private float spinDirection;
    private float spinTotalAngle;
    private float accumulatedRotation;

    private float nextDamageTime = 0f;
    [SerializeField] private float invulnerabilityWait = 0.5f; // Make in PlayerStats

    public float MoveSpeed => moveSpeed;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    public void SetAttackState(bool active)
    {
        isAttacking = active;
        animator.SetBool("Attacking", active);
    }

    public void DealDamageToOthers(int damage)
    {
        if (Time.time >= nextDamageTime)
        {
            PlayerStatsManager.instance.TakeDamage(damage);
            nextDamageTime = Time.time + invulnerabilityWait;
        }
    }

    public void SetupSpin(bool clockwise, bool facingUp)
    {
        // randomize half-spin count
        int halfSpinsRand = Random.Range(minHalfSpins, maxHalfSpins + 1);
        halfSpinsRand = Mathf.Clamp(halfSpinsRand, 1, 10);
        spinTotalAngle = 180f * halfSpinsRand;

        Vector2 dir = facingUp ? Vector2.up : Vector2.down;
        currentAngle = Vector2.SignedAngle(Vector2.right, dir);
        spinDirection = clockwise ? 1f : -1f;
        accumulatedRotation = 0f;
        IsRotating = true;
    }

    public void RotateAttackDirection()
    {
        if (!IsRotating) return;

        float step = spinSpeed * Time.deltaTime;
        currentAngle += step * spinDirection;
        accumulatedRotation += step;

        Vector2 attackDir = new Vector2(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad)
        ).normalized;

        animator.SetFloat("Horizontal", attackDir.x);
        animator.SetFloat("Vertical", attackDir.y);
        spriteRenderer.flipX = attackDir.x < 0f;

        if (accumulatedRotation >= spinTotalAngle)
        {
            IsRotating = false;
            SetAttackState(false);
        }
    }

    public float GetCurrentLaserAngle() => currentAngle;

    public void FaceDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
        spriteRenderer.flipX = direction.x < 0f;
    }
}