using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Stat Definition")]
public class StatDefinition : ScriptableObject
{
    public StatType statType;
    public string displayName;
    [TextArea(3, 8)]
    public string description;
    public Sprite icon;

    // --- static “database” ---
    private static Dictionary<StatType, StatDefinition> statDatabase;

    private static void EnsureLoaded()
    {
        if (statDatabase != null) return;

        statDatabase = new Dictionary<StatType, StatDefinition>();
        // Load ALL StatDefinition assets in a “Resources/Stats” folder:
        StatDefinition[] defs = Resources.LoadAll<StatDefinition>("Stats");
        foreach (StatDefinition d in defs)
        {
            if (!statDatabase.ContainsKey(d.statType))
                statDatabase[d.statType] = d;
        }
    }

    /// Call StatDefinition.Get(stat) to fetch the SO for any StatType.
    public static StatDefinition Get(StatType stat)
    {
        EnsureLoaded();
        return statDatabase.TryGetValue(stat, out StatDefinition d) ? d : null;
    }
}
