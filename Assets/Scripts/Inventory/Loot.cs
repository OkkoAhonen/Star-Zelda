using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Item item;

    public void Initialize(Item item)
    {
        this.item = item;
        sr.sprite = item.image;
    }

    // Start()-funktio tyhjennetty, koska alustus tehd��n Die()-funktiossa
    private void Start()
    {
        Initialize(this.item);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItem(item);
            StartCoroutine(MoveAndCollect(collision.transform));
        }
    }

    private IEnumerator MoveAndCollect(Transform target)
    {
        Destroy(boxCollider);

        while (transform.position != target.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null; // Korvattu 'yield return 0' modernimmalla 'null'-arvolla
        }

        Destroy(gameObject);
    }
}