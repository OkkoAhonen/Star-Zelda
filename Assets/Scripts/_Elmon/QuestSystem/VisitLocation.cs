using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VisitLocation : MonoBehaviour
{
    public string targetId; // e.g. "pillar_1", "pillar_2", etc.
    public bool oneTimeTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        Debug.Log("Visited: " + targetId);
        hasTriggered = true;
        QuestManager.instance.NotifyStepEvent("Visit", targetId);
    }
}
