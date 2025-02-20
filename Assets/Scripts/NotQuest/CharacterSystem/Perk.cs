using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Perk
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public Sprite Icon { get; private set; }
	public Dictionary<StatType, int> Requirements { get; private set; }
	public bool isTiered { get; private set; }
	public int maxLevel { get; private set; }
	public int requiredStatLevel { get; private set; }
	public int scalingFactor { get; private set; }
	public List<StatRequirement> statRequirements { get; private set; }
	public string perkName => Name;

	public Perk(string name, string description, Sprite icon, Dictionary<StatType, int> requirements, bool isTiered = false, int maxLevel = 1, int requiredStatLevel = 1, int scalingFactor = 1)
	{
		Name = name;
		Description = description;
		Icon = icon;
		Requirements = requirements;
		this.isTiered = isTiered;
		this.maxLevel = maxLevel;
		this.requiredStatLevel = requiredStatLevel;
		this.scalingFactor = scalingFactor;
		this.statRequirements = new List<StatRequirement>();
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

public class StatRequirement
{
	public StatType statType;
	public int requiredLevel;

	public StatRequirement(StatType type, int level)
	{
		statType = type;
		requiredLevel = level;
	}
}
