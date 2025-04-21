using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Laser : MonoBehaviour
{
    [Header("References (do not touch)")]
    [SerializeField] private Transform laserObject;
    [SerializeField] private Transform impactObject;

    private SpriteRenderer laserRenderer;
    private Animator laserAnimator;
    private BoxCollider2D laserCollider;

    private SpriteRenderer impactRenderer;
    private Animator impactAnimator;

    // Masks set by AI
    private LayerMask hitMask;
    private LayerMask damageMask;

    private float impactOffset = 0.05f;
    private float maxLaserLength = 15f;

    private bool isFiring = false;
    [SerializeField] private int laserDamage;

    public void SetMasks(LayerMask hitMask, LayerMask damageMask)
    {
        this.hitMask = hitMask;
        this.damageMask = damageMask;

        int laserLayer = laserObject.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & damageMask) == 0)
            Physics2D.IgnoreLayerCollision(laserLayer, i, true);
        }
    }

    public LayerMask HitMask => hitMask;

    private void Awake()
    {
        laserRenderer = laserObject.GetComponent<SpriteRenderer>();
        laserAnimator = laserObject.GetComponent<Animator>();
        laserCollider = laserObject.GetComponent<BoxCollider2D>();

        impactRenderer = impactObject.GetComponent<SpriteRenderer>();
        impactAnimator = impactObject.GetComponent<Animator>();

        laserRenderer.enabled = false;
        impactRenderer.enabled = false;
    }

    private void Update()
    {
        if (!isFiring) return;
        UpdateWidthAndImpact();
    }

    public void FireLaser()
    {
        laserAnimator.SetBool("Firing", true);
        impactAnimator.SetBool("Firing", true);

        isFiring = true;
        laserRenderer.enabled = true;
        impactRenderer.enabled = true;

        UpdateWidthAndImpact();
    }

    public void EndLaser()
    {
        laserAnimator.SetBool("Firing", false);
        impactAnimator.SetBool("Firing", false);
    }

    public void DisableLaser()
    {
        isFiring = false;
        laserRenderer.enabled = false;
        impactRenderer.enabled = false;
    }

    public void SetSortingOrder(int order)
    {
        laserRenderer.sortingOrder = order;
        impactRenderer.sortingOrder = order + 1;
    }

    private void UpdateWidthAndImpact()
    {
        Transform holder = laserObject.parent;
        float worldScale = holder.lossyScale.x;
        float worldMaxLen = maxLaserLength * worldScale;
        Vector2 baseOrigin = (Vector2)holder.position + (Vector2)(-holder.up * 0.01f);
        Vector2 baseDirection = -holder.up;

        RaycastHit2D hit = Physics2D.Raycast(baseOrigin, baseDirection, worldMaxLen, hitMask);
        bool usingOffsets = (holder.GetComponentInParent<BeholderAnimation>()?.useLaserOffsets ?? false);

        if (usingOffsets)
        {
            float rawDist = hit.collider != null ? hit.distance : worldMaxLen;
            Vector2 rawTip = hit.collider != null ? hit.point : baseOrigin + baseDirection * rawDist;

            Vector2 bossDelta = (Vector2)holder.parent.position - (Vector2)holder.position;
            float shiftAmt = Vector2.Dot(bossDelta, baseDirection);
            Vector2 tipWorld = rawTip + baseDirection * shiftAmt;

            float worldLen = Vector2.Distance(baseOrigin, tipWorld);
            float localLen = worldLen / worldScale;

            var lp = laserObject.localPosition;
            lp.y = -localLen * 0.5f;
            laserObject.localPosition = lp;
            laserRenderer.size = new Vector2(localLen, laserRenderer.size.y);
            if (laserCollider)
                laserCollider.size = new Vector2(localLen, laserCollider.size.y);

            var impactLocal = holder.InverseTransformPoint(tipWorld);
            impactLocal.y += impactOffset;
            impactLocal.z = impactObject.localPosition.z;
            impactObject.localPosition = impactLocal;
            impactRenderer.enabled = isFiring && hit.collider != null;
        }
        else
        {
            float worldLen = hit.collider != null ? hit.distance : worldMaxLen;
            float localLen = worldLen / worldScale;

            var lp = laserObject.localPosition;
            lp.y = -localLen * 0.5f;
            laserObject.localPosition = lp;

            laserRenderer.size = new Vector2(localLen, laserRenderer.size.y);
            if (laserCollider)
                laserCollider.size = new Vector2(localLen, laserCollider.size.y);

            Vector2 tipWorld = hit.collider != null ? hit.point : baseOrigin + baseDirection * worldLen;
            var impactLocal = holder.InverseTransformPoint(tipWorld);
            impactLocal.y += impactOffset;
            impactLocal.z = impactObject.localPosition.z;
            impactObject.localPosition = impactLocal;
            impactRenderer.enabled = isFiring && hit.collider != null;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isFiring) return;
        if (!other.CompareTag("Player")) return;
        if (((1 << other.gameObject.layer) & damageMask) == 0) return;

        transform.GetComponent<BossBase>().DealDamageToOthers(laserDamage);
    }

}