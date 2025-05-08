using UnityEngine;

[System.Serializable]
public class SBPatternStep
{
    public enum StepType
    {
        WalkToPlayer,
        Attack1,
        ComboAttack2,
        WalkToCenter,
        Attack3,
        Idle
    }

    public StepType stepType;

    [Header("Walk Settings (for WalkToPlayer / WalkToCenter)")]
    public bool staggerWhileWalking = false;
    // Damage needed to stagger mid-walk
    public float staggerDamageThreshold = 10f;
    // Number of subsequent attacks to skip when staggered
    public int staggerSkipAttacks = 1;

    [Header("Attack3 Settings")]
    // How long Attack3 lasts before ending
    public float attack3Duration = 2f;

    [Header("Idle Settings")]
    // Base idle duration (seconds)
    public float idleDuration = 3f;
}