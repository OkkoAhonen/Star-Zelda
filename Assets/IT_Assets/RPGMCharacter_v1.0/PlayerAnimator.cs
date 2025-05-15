using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerInputAnimator : MonoBehaviour
{
    private Animator animator;
    private float originalScaleX;

    private static readonly int HashHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int HashVertical = Animator.StringToHash("Vertical");
    private static readonly int HashMoving = Animator.StringToHash("Moving");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashPickup = Animator.StringToHash("Pickup");

    public GameObject walkingParticles;

    void Awake()
    {
        animator = GetComponent<Animator>();
        originalScaleX = transform.localScale.x;
    }

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal += 1f;

        if (horizontal == 0 && vertical == 0)
        {
            walkingParticles.SetActive(false);
        }
        else walkingParticles.SetActive(true);

        animator.SetFloat(HashHorizontal, horizontal);
        animator.SetFloat(HashVertical, vertical);

        bool isMoving = !Mathf.Approximately(horizontal, 0f)
                      || !Mathf.Approximately(vertical, 0f);
        animator.SetBool(HashMoving, isMoving);

        if (!Mathf.Approximately(horizontal, 0f))
        {
            Vector3 scale = transform.localScale;
            scale.x = (horizontal > 0f)
                      ? Mathf.Abs(originalScaleX)
                      : -Mathf.Abs(originalScaleX);
            transform.localScale = scale;
        }

        if (isMoving && walkingParticles != null)
        {
            float rawAngle = Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg;
            float zAngle = -rawAngle;
            walkingParticles.transform.localRotation =
              Quaternion.Euler(0f, 0f, zAngle);
        }

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger(HashAttack);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger(HashPickup);
        }
    }
}
