using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewPerk", menuName = "Game Data/Perk")]
public class Perk : ScriptableObject
{
	[SerializeField] private string _perkName;
	[SerializeField] private string _description;
	[SerializeField] private Sprite _icon;
	[SerializeField] private bool _isTiered; // ?
	[SerializeField] private int _maxLevel = 1; // How many times can be upgraded (1 means no upgrades)
	[SerializeField] private int _scalingFactor = 5; // How many more stat points needed per perk's level
	[SerializeField] private int _price = 100;
	[SerializeField] private List<StatRequirement> _statRequirements = new List<StatRequirement>();

	public string perkName => _perkName;
	public string description => _description;
	public Sprite icon => _icon;
	public bool isTiered => _isTiered;
	public int maxLevel => _maxLevel;
	public int scalingFactor => _scalingFactor;
	public int price => _price;
	public List<StatRequirement> statRequirements => _statRequirements;
}

[System.Serializable]
public class StatRequirement // What stat and how many levels of it?
{
	public StatType statType;
	public int requiredLevel;
}
