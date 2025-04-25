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

    public float impactOffset = 0.05f;
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
        Vector2 beamDir = -holder.up;

        // compute half-thickness in world units
        float halfThickWS = laserRenderer.size.y * worldScale * 0.5f;

        // origin nudged out by half thickness so we don’t start inside walls
        Vector2 originWS = (Vector2)holder.position + beamDir * halfThickWS;

        // raycast
        RaycastHit2D hit = Physics2D.Raycast(originWS, beamDir, maxWorldLen, hitMask);
        float rawDist = hit.collider ? hit.distance : maxWorldLen;

        // — OPTION A laser length: add half-thickness back in —
        float worldLen = rawDist + halfThickWS;
        float localLen = worldLen / worldScale;

        // resize beam
        laserRenderer.size = new Vector2(localLen, laserRenderer.size.y);
        if (laserCollider != null)
            laserCollider.size = new Vector2(localLen, laserCollider.size.y);

        // re-anchor beam so its base stays at the holder
        laserObject.localPosition = new Vector3(
            0f,
            -localLen * 0.5f,
            laserObject.localPosition.z
        );

        // now compute **visual** tip: raw hit point + half-thickness
        Vector2 rawTip = originWS + beamDir * rawDist;
        Vector2 visualTip = rawTip + beamDir * halfThickWS;

        // place impact at that visual tip, then back off by your impactOffset
        float impactWSOffset = impactOffset * worldScale;
        impactObject.position = visualTip
            - beamDir.normalized * impactWSOffset;

        // toggle visibility
        laserRenderer.enabled = isFiring;
        impactRenderer.enabled = isFiring && hit.collider != null;
    }
}