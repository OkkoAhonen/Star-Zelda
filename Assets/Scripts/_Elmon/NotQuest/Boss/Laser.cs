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
        DisableLaser();
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
        float maxWorldLen = maxLaserLength * worldScale;
        bool usingOffsets = holder.GetComponentInParent<BeholderAnimation>()?.useLaserOffsets ?? false;

        Vector2 beamOrigin;
        Vector2 beamDir = -holder.up;

        if (usingOffsets)
        {
            // —— OFFSETTED origin: back up from holder by its localPosition —— 
            Vector2 localOffset = holder.localPosition;
            Vector2 worldOffset = holder.TransformVector(localOffset);
            beamOrigin = (Vector2)holder.position - worldOffset;
        }
        else
        {
            // —— SIMPLE origin: exactly at the holder’s world?position ——
            beamOrigin = holder.position;
        }

        // — common raycast & sizing logic —
        RaycastHit2D hit = Physics2D.Raycast(beamOrigin, beamDir, maxWorldLen, hitMask);
        float worldLen = hit.collider != null ? hit.distance : maxWorldLen;
        float localLen = worldLen / worldScale;

        // resize beam
        laserRenderer.size = new Vector2(localLen, laserRenderer.size.y);
        if (laserCollider)
            laserCollider.size = new Vector2(localLen, laserCollider.size.y);

        // keep the beam’s base anchored at beamOrigin
        var lp = laserObject.localPosition;
        lp.y = -localLen * 0.5f;
        laserObject.localPosition = lp;

        // place impact
        Vector2 worldTip = hit.collider != null
            ? hit.point
            : beamOrigin + beamDir * worldLen;

        impactObject.position = worldTip + (Vector2)(laserObject.up) * impactOffset;
        impactRenderer.enabled = isFiring && hit.collider != null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isFiring) return;
        if (!other.CompareTag("Player")) return;
        if (((1 << other.gameObject.layer) & damageMask) == 0) return;

        transform.GetComponent<BossBase>().DealDamageToOthers(laserDamage);
    }

}