using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Perk
{
	[SerializeField] private string name;
	[SerializeField] private string description;
	[SerializeField] private Sprite icon;
	[SerializeField] private bool _isTiered;
	[SerializeField] private int _maxLevel = 1;
	[SerializeField] private int _requiredStatLevel = 1;
	[SerializeField] private int _scalingFactor = 1;
	[SerializeField] private List<StatRequirement> _statRequirements = new List<StatRequirement>();

	// Properties that use the serialized fields
	public string Name => name;
	public string Description => description;
	public Sprite Icon => icon;
	public bool isTiered => _isTiered;
	public int maxLevel => _maxLevel;
	public int requiredStatLevel => _requiredStatLevel;
	public int scalingFactor => _scalingFactor;
	public List<StatRequirement> statRequirements => _statRequirements;
	public string perkName => name;

	// Remove the constructor or make it private if you want to force creation through the Inspector
	private Perk() { }

	public bool CanUnlock(PlayerStats playerStats)
	{
		foreach (var requirement in statRequirements)
		{
			if (playerStats.GetStat(requirement.statType) < requirement.requiredLevel)
				return false;
		}
		return true;
	}
}

[System.Serializable]
public class StatRequirement
{
	public StatType statType;
	public int requiredLevel;
}
