using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewPerk", menuName = "Game Data/Perk")]
public class Perk : ScriptableObject // Inherits from ScriptableObject
{
	[SerializeField] private string description;
	[SerializeField] private Sprite icon;
	[SerializeField] private bool _isTiered;
	[SerializeField] private int _maxLevel = 1;
	[SerializeField] private int _requiredStatLevel = 1;
	[SerializeField] private int _scalingFactor = 1;
	[SerializeField] private int price = 100;
	[SerializeField] private List<StatRequirement> _statRequirements = new List<StatRequirement>();

	// Properties that use the serialized fields
	public string Name => name;
	public string Description => description;
	public Sprite Icon => icon;
	public bool isTiered => _isTiered;
	public int maxLevel => _maxLevel;
	public int requiredStatLevel => _requiredStatLevel;
	public int scalingFactor => _scalingFactor;
	public int Price => price;
	public List<StatRequirement> statRequirements => _statRequirements;
	public string perkName => name;

	public bool CanUnlock(PlayerStatsManager playerStats)
	{// Check if the player meets all stat requirements to unlock this perk
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
