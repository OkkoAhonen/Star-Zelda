using UnityEngine;

public class StaticValueManager : MonoBehaviour
{
    public static StaticValueManager instance { get; private set; }

    [Header("Masks (Set in Inspector)")]
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private LayerMask _damageNonEnemiesMask;
    [SerializeField] private LayerMask _damageEnemiesMask;

    public static LayerMask HitMask { get; private set; }
    public static LayerMask DamageNonEnemiesMask { get; private set; }
    public static LayerMask DamageEnemiesMask { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            HitMask = _hitMask;
            DamageNonEnemiesMask = _damageNonEnemiesMask;
            DamageEnemiesMask = _damageEnemiesMask;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}