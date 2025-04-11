using System;
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem
{
	private Dictionary<Perk, int> unlockedPerks = new Dictionary<Perk, int>();
	private PlayerStatsManager playerStatsManager;

	public event Action<Perk> OnPerkUnlocked;

	public PerkSystem(PlayerStatsManager statsManager)
	{
		playerStatsManager = statsManager;
		playerStatsManager.onStatChanged += CheckPerkUnlocks;
	}

	private void CheckPerkUnlocks()
	{ // Check all perks if any can be upgraded or unlocked
		foreach (var perk in PerkDatabase.Instance.AllPerks)
		{
			TryUnlockPerk(perk);
		}
	}

	private void TryUnlockPerk(Perk perk)
	{
		// Try to upgrade unlocked perk
		if (unlockedPerks.ContainsKey(perk) && perk.isTiered && unlockedPerks[perk] < perk.maxLevel)
		{
			int requiredLevel = perk.requiredStatLevel + (unlockedPerks[perk] * perk.scalingFactor);

			if (MeetsStatRequirements(perk, requiredLevel))
			{
				unlockedPerks[perk]++;
				Debug.Log($"Upgraded Perk: {perk.perkName} to Level {unlockedPerks[perk]}");
			}
		} // Unlocks perk if stats are high enough and you don't have it yet
		else if (!unlockedPerks.ContainsKey(perk) && MeetsStatRequirements(perk, perk.requiredStatLevel))
		{
			unlockedPerks[perk] = 1;
			Debug.Log($"Unlocked Perk: {perk.perkName}");
			OnPerkUnlocked?.Invoke(perk);
		}
	}

	private bool MeetsStatRequirements(Perk perk, int requiredLevel)
	{	// Unlock perk if player's stats are high enough
		foreach (var req in perk.statRequirements)
		{
			if (playerStatsManager.GetStat(req.statType) < req.requiredLevel)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasPerk(Perk perk) => unlockedPerks.ContainsKey(perk);
	public int GetPerkLevel(Perk perk) => unlockedPerks.ContainsKey(perk) ? unlockedPerks[perk] : 0;
}
