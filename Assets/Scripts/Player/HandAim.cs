using UnityEngine;

public class HandAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hand;        // Käsi
    [SerializeField] private Transform pivotPoint;  // The point to rotate around
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
        RotateAroundPivotTowardMouse();
    }

    void HandleMovement()
    {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");

        moveDirection = new Vector2(x, y).normalized;
        transform.position += (Vector3)(moveDirection * playerMoveSpeed * Time.deltaTime); // World-space movement
    }

    void RotateAroundPivotTowardMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 pivotToMouse = mousePos - pivotPoint.position;
        float angle = Mathf.Atan2(pivotToMouse.y, pivotToMouse.x) * Mathf.Rad2Deg;

        // Set the player's position to orbit around the pivot
        float distance = Vector3.Distance(transform.position, pivotPoint.position);
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * distance;

        // Rotate the player to face the pivot or mouse — optional depending on look
        pivotPoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
