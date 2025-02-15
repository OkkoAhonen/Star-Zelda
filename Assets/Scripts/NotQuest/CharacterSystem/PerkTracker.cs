using System.Collections.Generic;
using UnityEngine;

public class PerkTracker
{
	private Dictionary<string, int> killCounts = new Dictionary<string, int>();
	private List<Perk> unlockedPerks = new List<Perk>();

	// Define milestone-based unlocks
	private Dictionary<string, (int requiredKills, string perkName)> killMilestones = new Dictionary<string, (int, string)>
	{
		{ "Wolf", (200, "Wolf Slayer") },
		{ "Bandit", (150, "Bandit Hunter") },
		{ "Dragon", (5, "Dragon Slayer") }
	};

	// Track when an enemy is killed
	public void RecordKill(string enemyType)
	{
		if (!killCounts.ContainsKey(enemyType))
			killCounts[enemyType] = 0;

		killCounts[enemyType]++;

		// Check if we reached a milestone for a perk
		if (killMilestones.TryGetValue(enemyType, out var milestone))
		{
			if (killCounts[enemyType] >= milestone.requiredKills && !HasPerk(milestone.perkName))
			{
				UnlockPerk(milestone.perkName);
			}
		}
	}

	// Unlock a new perk
	private void UnlockPerk(string perkName)
	{
		Perk newPerk = PerkDatabase.GetPerkByName(perkName);
		if (newPerk != null)
		{
			unlockedPerks.Add(newPerk);
			Debug.Log("Unlocked Perk: " + perkName);
		}
	}

	// Check if a perk is already unlocked
	public bool HasPerk(string perkName)
	{
		return unlockedPerks.Exists(perk => perk.perkName == perkName);
	}
}
