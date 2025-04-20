using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    private PlayerStats stats;
    private Camera cam;

    public GameObject rotationFix;
    public Quaternion rotationFixRotation; // Muutettu 'quaternion' -> 'Quaternion'

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Pääkameraa ei löytynyt!");
        }
    }

    void Start()
    {
        if (rotationFix != null)
        {
            rotationFixRotation = rotationFix.transform.rotation;
        }
        else
        {
            Debug.LogWarning("rotationFix ei ole määritelty!");
        }

        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();

        if (stats == null)
        {
            Debug.LogError("PlayerStats-komponenttia ei löytynyt tästä GameObjectista!");
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D-komponenttia ei löytynyt tästä GameObjectista!");
        }
    }

    void Update()
    {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");

        Vector2 moveDirection = new Vector2(x, y).normalized;

        rb.velocity = moveDirection * stats.playerMoveSpeed; // Poistettu Time.deltaTime

        PlayerMouseRotation();
    }

    public void PlayerMouseRotation()
    {
        if (cam == null) return;

        Vector3 mousepos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousepos.z = 0;

        float angleRad = Mathf.Atan2(mousepos.y - transform.position.y, mousepos.x - transform.position.x); // Muutettu math -> Mathf
        float angleDeg = (180 / Mathf.PI) * angleRad; // Muutettu math -> Mathf

        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        Debug.DrawLine(transform.position, mousepos, Color.red, Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (rotationFix != null)
        {
            rotationFix.transform.rotation = rotationFixRotation;
        }
    }
}