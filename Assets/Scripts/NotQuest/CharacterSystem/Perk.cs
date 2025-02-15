using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Perk
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public Sprite Icon { get; private set; }
	public Dictionary<StatType, int> Requirements { get; private set; }

	public Perk(string name, string description, Sprite icon, Dictionary<StatType, int> requirements)
	{
		Name = name;
		Description = description;
		Icon = icon;
		Requirements = requirements;
	}

	public bool CanUnlock(PlayerStats playerStats)
	{
		foreach (var requirement in Requirements)
		{
			if (playerStats.GetStat(requirement.Key) < requirement.Value)
				return false;
		}
		return true;
	}
}
