using System;
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem
{
    public static PerkSystem instance { get; private set; }
	private Dictionary<Perk, int> unlockedPerks = new Dictionary<Perk, int>();
	private PlayerStatsManager playerStatsManager;

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
		foreach (var perk in PerkDatabase.Instance.AllPerks)
		{
			TryUnlockPerk(perk);
		}
	}

	public bool TryUnlockPerk(Perk perk)
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
			return true;
		}
		return false;
	}

	private bool MeetsStatRequirements(Perk perk, int requiredLevel)
	{	// Unlock perk if player's stats are high enough
		foreach (var req in perk.statRequirements)
		{
			if (PlayerStatsManager.instance.GetStat(req.statType) < req.requiredLevel)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasPerk(Perk perk) => unlockedPerks.ContainsKey(perk);
	public int GetPerkLevel(Perk perk) => unlockedPerks.ContainsKey(perk) ? unlockedPerks[perk] : 0;
}
