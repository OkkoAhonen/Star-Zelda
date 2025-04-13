using System;
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem
{
    public static PerkSystem instance { get; private set; }
	public int perksBought { get; private set; }
	private Dictionary<Perk, int> unlockedPerks = new Dictionary<Perk, int>(); // int is level of perk
    public List<Perk> allPerks = new List<Perk>();

	public event Action<Perk> OnPerkUnlocked;

    public PerkSystem()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogWarning("Multiple instances of PerkSystem created!");
    }

	private void CheckPerkUnlocks()
	{ // Check all perks if any can be upgraded or unlocked
		foreach (var perk in allPerks)
		{
			TryUnlockPerk(perk);
		}
	}

	public bool TryUnlockPerk(Perk perk)
	{
		if (PlayerStatsManager.instance.perkPurchasesAvailable == 0)
		{
			Debug.Log($"You can get a perk every 3 levels. You've bought: {perksBought}");
			return false;
		}

		// Try to upgrade unlocked perk
		if (unlockedPerks.ContainsKey(perk) && unlockedPerks[perk] < perk.maxLevel)
		{
		int scaling = unlockedPerks[perk] * perk.scalingFactor; // requiredStatLevel + unlockedPerks' level * multiplier (e.g. 10 + 1 * 5 = 15)

			if (MeetsStatRequirements(perk, scaling))
			{
				unlockedPerks[perk]++;
				Debug.Log($"Upgraded Perk: {perk.perkName} to Level {unlockedPerks[perk]}");
			}
		} // Unlocks perk if stats are high enough and you don't have it yet
		else if (!unlockedPerks.ContainsKey(perk) && MeetsStatRequirements(perk, 0)) // No scaling need if you don't have it
		{
			unlockedPerks[perk] = 1;
			Debug.Log($"Unlocked Perk: {perk.perkName}");
			OnPerkUnlocked?.Invoke(perk);
			perksBought++;
			return true;
		}
		Debug.Log("Doesn't meet requirements?");
		return false;
	}

	private bool MeetsStatRequirements(Perk perk, int scaling)
	{	// Unlock perk if player's stats are high enough
		foreach (var req in perk.statRequirements)
		{
			var type = req.statType;
			int scaledRequiredLevel = req.requiredLevel + scaling;
			int currentLevel = PlayerStatsManager.instance.GetStat(type);

			if (currentLevel < scaledRequiredLevel)
			{
				Debug.Log($"{type} {currentLevel} is lower than {scaledRequiredLevel}");
				return false;
			}
		}
		return true;
	}

	public bool HasPerk(Perk perk) => unlockedPerks.ContainsKey(perk);
	public int GetPerkLevel(Perk perk) => unlockedPerks.ContainsKey(perk) ? unlockedPerks[perk] : 0;
}
