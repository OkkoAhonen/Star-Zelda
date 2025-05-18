using System.Collections;
using UnityEngine;

public class LightningScript : MonoBehaviour
{
    private Animator animator;
    private BoxCollider2D boxCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(TriggerLightningLoop());

        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }

    private IEnumerator TriggerLightningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(8f);
            animator.SetTrigger("Light");
        }
    }

    public void HitboxOn()
    {
        Debug.Log("Put the hitbox on");
        boxCollider.enabled = true;

    }
    public void HitboxOff()
    {
        Debug.Log("Put the hitbox off");
        boxCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Take Damege");
    }
}
