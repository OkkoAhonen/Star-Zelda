using System.Collections;
using UnityEngine;

// Simple loot pickup that moves toward player and adds to InventoryManager
public class Loot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Item item;

    public void Initialize(Item itemToGive)
    {
        item = itemToGive;
        if (sr != null && item != null) sr.sprite = item.Image;
    }

    private void Start()
    {
        if (item != null && sr != null) sr.sprite = item.Image;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Use static mask if assigned; fallback to tag check
        if ((StaticValueManager.DamageNonEnemiesMask.value & (1 << collision.gameObject.layer)) != 0 ||
            collision.gameObject.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item);
            }
            StartCoroutine(MoveAndCollect(collision.transform));
        }
    }

    private IEnumerator MoveAndCollect(Transform target)
    {
        if (boxCollider != null) Destroy(boxCollider);
        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}
