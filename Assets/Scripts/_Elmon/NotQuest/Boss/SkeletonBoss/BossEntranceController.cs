using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class BossEntranceController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tag on the boss GameObject")]
    [SerializeField] private string bossTag = "Boss";
    [Tooltip("Camera pan speed")]
    [SerializeField] private float panSpeed = 5f;
    [Tooltip("Delay before resurrection")]
    [SerializeField] private float dramaticPauseTime = 1f;

    private CameraFollow camFollow;
    private SkeletonBossAI bossAI;
    private Animator bossAnimator;
    private bool triggered;

    private void Awake()
    {
        var bossGO = GameObject.FindWithTag(bossTag);
        if (bossGO == null)
            Debug.LogError($"No GameObject tagged '{bossTag}' found.");
        else
        {
            bossAI = bossGO.GetComponent<SkeletonBossAI>();
            bossAnimator = bossGO.GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || other.tag != "Player") return;
        triggered = true;

        camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow) camFollow.enabled = false;

        StartCoroutine(EntranceSequence());
    }

    private IEnumerator EntranceSequence()
    {
        // pan camera to the boss’s arenaCenter
        var cam = Camera.main.transform;
        Vector3 target = bossAI.arenaCenter;
        target.z = cam.position.z;

        while ((Vector2)cam.position != bossAI.arenaCenter)
        {
            cam.position = Vector3.MoveTowards(
                cam.position, target, panSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dramaticPauseTime);

        bossAI.Resurrect();

        yield return null;

        int layer = 0;
        AnimatorStateInfo entered = bossAnimator.GetCurrentAnimatorStateInfo(layer);
        int clipHash = entered.shortNameHash;

        yield return new WaitWhile(() =>
        {
            var s = bossAnimator.GetCurrentAnimatorStateInfo(layer);
            return s.shortNameHash == clipHash && s.normalizedTime < 1f;
        });

        if (camFollow) camFollow.enabled = true;
        Destroy(gameObject);
    }
}