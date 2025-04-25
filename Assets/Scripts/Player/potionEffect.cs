using System.Collections;
using UnityEngine;

public class potionEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    public Color startColor = Color.cyan;
    public Color endColor = Color.red;
    public float duration = 5f; // Aika, jonka kuluessa v�ri muuttuu

    private float timer = 0f;
    //private float damageDuration = .5f;
    private bool canDamage = true;

    [SerializeField] private GameObject player;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = startColor;
        player = GameObject.FindWithTag("Player");

        StartCoroutine(ChangeColorOverTime());
    }

    IEnumerator ChangeColorOverTime()
    {
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            Color newColor = Color.Lerp(startColor, endColor, t);
            newColor.a = Mathf.Lerp(1f, 0f, t);
            sr.color = newColor;

            yield return null;
        }

        if(timer >= duration)
        {
            Destroy(gameObject);
        }
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

        if (collision.CompareTag("Enemy") && canDamage && equippedItem.isDamagePotion)
        {
            EnemyController enemyhealth = collision.GetComponent<EnemyController>();
            enemyhealth.TakeDamage(equippedItem.potionAttackDamage, Vector2.zero, 0f);
            canDamage = false;
            StartCoroutine(PotionDamageCooldown());
        }
        else if (collision.CompareTag("Player") && equippedItem.isHealthPotion && canDamage)
        {
            playerAction playeraction = collision.gameObject.GetComponent<playerAction>();
            playeraction.playerTakeDamage(-equippedItem.PotionHeal);
            StartCoroutine(PotionDamageCooldown());
  
        }
    }

    private IEnumerator PotionDamageCooldown()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        yield return new WaitForSeconds(equippedItem.damageDuration);
        canDamage = true;
    }



}
