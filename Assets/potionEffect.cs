using System.Collections;
using UnityEngine;

public class potionEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    public Color startColor = Color.cyan;
    public Color endColor = Color.red;
    public float duration = 5f; // Aika, jonka kuluessa väri muuttuu

    private float timer = 0f;
    private float damageDuration = .5f;
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
            sr.color = Color.Lerp(startColor, endColor, t);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        if(timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (canDamage) {
                
                EnemyAI enemyAI = collision.GetComponent<EnemyAI>();

                PotionAttack potionAttack = player.GetComponent<PotionAttack>();
                enemyAI.health -= potionAttack.potionDamage;
                Debug.Log(enemyAI.health);
                canDamage = false;
            }
            StartCoroutine(PotionDamageCooldown());
        }
    }

    private IEnumerator PotionDamageCooldown()
    {
        yield return new WaitForSeconds(damageDuration);
        canDamage = true;
    }



}
