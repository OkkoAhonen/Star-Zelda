using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public GameObject sword; // Viittaus miekan GameObjectiin.
    public SpriteRenderer swordRenderer; // SpriteRenderer-komponentti miekan kuvan vaihtamiseksi.

    public float swingSpeed = 200f; // Miekan heilautusnopeus.
    private bool isSwinging = false; // Tieto siitä, onko miekka parhaillaan heilautuksessa.
    private Vector2 attackDirection = Vector2.right; // Lyönnin suunta (oletuksena oikealle).
    public PlayerMovement2D playerMovement;

    [SerializeField] private Transform UpSwing;
    [SerializeField] private Transform DownSwing;
    [SerializeField] private Transform LeftSwing;
    [SerializeField] private Transform RightSwing;

    [Header("Swing Settings")]
    public float swingDistance = 2.0f;
    public float swingDuration = 0.5f;
    private Vector3 swingEndPosition;

    public Sprite sprite2;

    void Start()
    {

        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned!");
        }
        if (swordRenderer == null)
        {
            Debug.LogError("Sword SpriteRenderer is not assigned!");
        }

        
    }

    void Update()
    {
        HandleAttackDirection();

        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartCoroutine(SwingSword());
        }
        UpdateSwordAppearance();
    }

    void HandleAttackDirection()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            attackDirection = Vector2.up;
            sword.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            attackDirection = Vector2.down;
            sword.transform.rotation = Quaternion.Euler(0, 0, -180);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            attackDirection = Vector2.left;
            
            sword.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            attackDirection = Vector2.right;
            sword.transform.rotation = Quaternion.Euler(0, 0, -90);
        }

        UpdateSwordPosition();
    }

    void UpdateSwordPosition()
    {
        if (attackDirection == Vector2.up)
        {
            sword.transform.position = UpSwing.position;
        }
        else if (attackDirection == Vector2.down)
        {
            sword.transform.position = DownSwing.position;
        }
        else if (attackDirection == Vector2.left)
        {
            sword.transform.position = LeftSwing.position;
        }
        else if (attackDirection == Vector2.right)
        {
            sword.transform.position = RightSwing.position;
        }
    }

    IEnumerator SwingSword()
    {
        isSwinging = true;
        Vector3 initialPosition = sword.transform.position;
        swingEndPosition = initialPosition + (Vector3)attackDirection * swingDistance;
        float elapsedTime = 0f;

        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(initialPosition, swingEndPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sword.transform.position = swingEndPosition;
        elapsedTime = 0f;

        while (elapsedTime < swingDuration)
        {
            sword.transform.position = Vector3.Lerp(swingEndPosition, initialPosition, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sword.transform.position = initialPosition;
        isSwinging = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Sword hit enemy {other.name}");
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
                if (equippedItem != null && equippedItem.isWeapon)
                {
                    float damage = equippedItem.attackDamage;

                    enemyStats.maxHealth = Mathf.Max(0, enemyStats.maxHealth - damage);
                    Debug.Log($"Hit enemy! Damage: {damage}, Remaining health: {enemyStats.maxHealth}");

                    if (enemyStats.maxHealth == 0)
                    {
                        other.GetComponent<EnemyController>()?.Death();
                    }
                }
                else
                {
                    Debug.LogWarning("No weapon equipped or item is not a weapon!");
                }
            }
            else
            {
                Debug.LogWarning("Enemy does not have an EnemyStats component.");
            }
        }
    }

    public void UpdateSwordAppearance()
    {

        

        //wordRenderer.sprite = sprite2;
        // Hakee aktiivisesti valitun esineen InventoryManagerista.
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

        
        


            if (equippedItem != null && equippedItem.isWeapon)
        {
            // Päivittää miekan Sprite-kuvan vastaamaan esineen spriteä.
            swordRenderer.sprite = equippedItem.image;
        }
        else
        {
            Debug.LogWarning("No weapon equipped, or equipped item does not have an image!");
        }
    }
}
