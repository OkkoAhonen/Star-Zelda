using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Playables;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    //pelaajan käden paikannus jousipyssyn sijaintia varten
    [SerializeField] Transform hand;

    //playerStats kansio pelaajan tiedot
    PlayerStats stats;

    private Camera cam;


    //Estää visuaalinen rotaation
    public GameObject rotationFix;
    public quaternion rotationFixRotation;

    // Start is called before the first frame update

    private void Awake()
    {
        cam = Camera.main;
    }
    void Start()
    {
        rotationFixRotation = rotationFix.transform.rotation;

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


    // Update is called once per frame
    void Update()
    {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");

        Vector2 moveDirection = new Vector2(x, y).normalized;


        rb.velocity = moveDirection * stats.playerMoveSpeed;

        PlayerMouseRotation();
    }

    private void Reset()
    {
        float angle = Utility.AngleTowardsMouse(hand.position);
        hand.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    public void PlayerMouseRotation() {

        Vector3 mousepos = (Vector2)cam.ScreenToWorldPoint( Input.mousePosition);

        
        float angleRad = math.atan2(mousepos.y - transform.position.y, mousepos.x - transform.position.x);
        float angleDeg = (180 / math.PI) * angleRad ;

        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        Debug.DrawLine(transform.position, mousepos, Color.red, Time.deltaTime);
    }

    private void LateUpdate()
    {
        rotationFix.transform.rotation = rotationFixRotation;
    }
}
