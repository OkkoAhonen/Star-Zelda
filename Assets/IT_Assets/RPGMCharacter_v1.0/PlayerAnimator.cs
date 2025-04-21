using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerInputAnimator : MonoBehaviour
{
    private Animator animator;
    private float originalScaleX;

    // Animator parameter hashes
    private static readonly int HashHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int HashVertical = Animator.StringToHash("Vertical");
    private static readonly int HashMoving = Animator.StringToHash("Moving");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashPickup = Animator.StringToHash("Pickup");

    void Awake()
    {
        animator = GetComponent<Animator>();
        originalScaleX = transform.localScale.x;
    }

    void Update()
    {
        // Get WASD input
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal += 1f;

        // Update animator floats
        animator.SetFloat(HashHorizontal, horizontal);
        animator.SetFloat(HashVertical, vertical);

        // Update moving boolean
        bool isMoving = !Mathf.Approximately(horizontal, 0f) || !Mathf.Approximately(vertical, 0f);
        animator.SetBool(HashMoving, isMoving);

        // Flip based on horizontal input
        if (!Mathf.Approximately(horizontal, 0f))
        {
            Vector3 scale = transform.localScale;
            scale.x = (horizontal > 0f) ? Mathf.Abs(originalScaleX) : -Mathf.Abs(originalScaleX);
            transform.localScale = scale;
        }

        // Attack on left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger(HashAttack);
        }

        // Pickup on 'C' key down
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger(HashPickup);
        }
    }
}