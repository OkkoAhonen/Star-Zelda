using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float respawnTimeSeconds = 8;
    [SerializeField] private int GainExperience = 25;

    private CircleCollider2D circleCollider;
    private SpriteRenderer visual;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        visual = GetComponentInChildren<SpriteRenderer>();
    }

    private void CollectGem()
    {
        circleCollider.enabled = false;
        visual.gameObject.SetActive(false);
        GameEventsManager.instance.playerEvents.GainExperience(GainExperience);
        StopAllCoroutines();
        StartCoroutine(RespawnAfterTime());
    }

    private IEnumerator RespawnAfterTime()
    {
        yield return new WaitForSeconds(respawnTimeSeconds);
        circleCollider.enabled = true;
        visual.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectGem();
            if (respawnTimeSeconds == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}