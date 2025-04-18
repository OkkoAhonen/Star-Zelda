using UnityEngine;

public class HandAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hand;        // Käsi
    [SerializeField] private Camera mainCam;        // Main camera

    [Header("Movement Settings")]
    [SerializeField] private float playerMoveSpeed = 5f; // Speed of player movement

    [Header("Debug")]
    [SerializeField] private Vector2 moveDirection; // Current movement direction

    public Vector2 MoveDirection => moveDirection;

    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        RotatePlayerTowardMouse();
    }

    void HandleMovement()
    {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");

        moveDirection = new Vector2(x, y).normalized;

        transform.position += (Vector3)(moveDirection * playerMoveSpeed * Time.deltaTime); // World-space movement
    }


    void RotatePlayerTowardMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Aim hand at mouse
    }
}
