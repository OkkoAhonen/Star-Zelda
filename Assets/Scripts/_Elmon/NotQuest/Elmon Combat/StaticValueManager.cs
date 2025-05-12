using UnityEngine;

public class StaticValueManager : MonoBehaviour
{
    public static StaticValueManager instance { get; private set; }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
    }

    [Header("Masks")]
    public static LayerMask HitMask { get; private set; }
    public static LayerMask DamageNonEnemiesMask { get; private set; }
    public static LayerMask DamageEnemiesMask { get; private set; }
}
