using UnityEngine;

[System.Serializable]
public class SBPatternStep
{
    public enum StepType
    {
        WalkToPlayer,
        WalkToCenter,
        RandomSpot,               // new: pick a random point in the arena
        Attack1,
        Attack3,
        Idle
    }

    public StepType stepType;

    [Header("Movement")]
    // per-step speed multiplier (default 1)
    public float speedMultiplier = 1f;

    [Header("Walk ? Player")]
    public float approachRandomOffset = 0f;

    [Header("Attack1 Settings")]
    public bool combo = false;          // if true, play the combo ending

    [Header("Stagger (Walk steps)")]
    public bool staggerWhileWalking = false;
    public float staggerDamageThreshold = 10f;
    public int staggerSkipAttacks = 2;

    [Header("Attack3 Settings")]
    public float attack3Duration = 5f;
    public float crystalSpawnRate = 8f;
    public float crystalHealth = 15f;

    [Header("Idle Settings")]
    public float idleDuration = 3f;
}