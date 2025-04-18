using System.Collections.Generic;
using UnityEngine;

public class TraitManager : MonoBehaviour
{
    public static TraitManager instance { get; private set; }

    private Dictionary<string, int> killCounts = new();
    private List<Trait> unlockedTraits = new();
    private PlayerEvents playerEvents;

    // You can configure this list via the inspector or manually register them
    [SerializeField] private List<Trait> allTraits;

    // Map enemy type → (requiredKills, traitName)
    private Dictionary<string, (int requiredKills, string traitName)> killMilestones = new()
    {
        { "Wolf", (1, "Wolf Slayer") },
        { "Bandit", (150, "Bandit Hunter") },
        { "Dragon", (5, "Dragon Slayer") }
    };

    private Dictionary<string, Trait> traitByName = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        playerEvents = GameEventsManager.instance.playerEvents;
        playerEvents.onEnemyKilled += RecordKill;

        CacheTraits();
    }

    private void CacheTraits()
    {
        foreach (var trait in allTraits)
        {
            if (trait != null && !string.IsNullOrEmpty(trait.traitName))
                traitByName[trait.traitName] = trait;
        }
    }

    private void RecordKill(string enemyType)
    {
        Debug.Log("Kill Recorded: " + enemyType);

        if (!killCounts.ContainsKey(enemyType))
            killCounts[enemyType] = 0;

        killCounts[enemyType]++;

        if (killMilestones.TryGetValue(enemyType, out var milestone))
        {
            if (killCounts[enemyType] >= milestone.requiredKills && !HasTrait(milestone.traitName))
            {
                UnlockTrait(milestone.traitName);
            }
        }
    }

    private void UnlockTrait(string traitName)
    {
        if (traitByName.TryGetValue(traitName, out var trait))
        {
            unlockedTraits.Add(trait);
            Debug.Log($"Unlocked Trait: {trait.traitName}");
            // You could trigger an event here if you want to notify UI or systems
        }
        else
        {
            Debug.LogWarning($"Trait not found: {traitName}");
        }
    }

    public bool HasTrait(string traitName)
    {
        return unlockedTraits.Exists(t => t.traitName == traitName);
    }

    public bool HasTrait(Trait trait)
    {
        return unlockedTraits.Contains(trait);
    }

    public List<Trait> GetUnlockedTraits() => new(unlockedTraits);

    private void OnDestroy()
    {
        if (playerEvents != null)
            playerEvents.onEnemyKilled -= RecordKill;
    }
}
