using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SplashArea : MonoBehaviour
{
    // damage per tick
    public float damage = 10f;
    // if > 0, live for exactly this many seconds; if ? 0 and hasEndAnimation, wait for animation
    public float lifespan = 0f;
    // scale multiplier applied on each enable
    public float sizeMultiplier = 1f;
    // if true, will trigger "endLoop" on the animator when lifespan ? 0
    public bool hasEndAnimation = false;
    // which layers to damage
    public LayerMask damageMask;
    // optional animator reference (will be grabbed in Awake)
    public Animator animator;

    // set by PoolingManager when instantiated
    [HideInInspector] public GameObject prefabReference;

    // objects currently inside this area
    private HashSet<GameObject> _inside = new HashSet<GameObject>();
    // original unmodified scale
    private Vector3 _baseScale;

    void Awake()
    {
        // cache the unscaled size and grab the animator
        _baseScale = transform.localScale;
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // clear any old hits and reset size
        _inside.Clear();
        transform.localScale = _baseScale * sizeMultiplier;
        // begin the timed or animation?based lifecycle
        StartCoroutine(Lifecycle());
    }

    void OnDisable()
    {
        // ensure no leftover references
        _inside.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int bit = 1 << other.gameObject.layer;
        if ((bit & damageMask) != 0)
        {
            _inside.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _inside.Remove(other.gameObject);
    }

    void Update()
    {
        // apply damage each frame to everything inside
        foreach (var go in _inside)
        {
            if (go != null)
            {
                // TODO: apply damage to 'go'
            }
        }
    }

    private IEnumerator Lifecycle()
    {
        if (lifespan > 0f)
        {
            // wait fixed duration
            yield return new WaitForSeconds(lifespan);
        }
        else if (hasEndAnimation && animator != null)
        {
            // wait until the one?shot animation finishes
            yield return EndAnimation();
        }

        // cleanup state
        _inside.Clear();

        // return to pool or simply deactivate
        if (prefabReference != null && PoolingManager.Instance != null)
        {
            PoolingManager.Instance.ReturnToPool(prefabReference, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator EndAnimation()
    {
        // trigger your "endLoop" transition
        animator.SetTrigger("endLoop");
        yield return null;
        // wait the remainder of the current clip
        var clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
        {
            yield return new WaitForSeconds(clips[0].clip.length - 0.1f);
        }
    }
}