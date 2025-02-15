using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkDatabase", menuName = "Game Data/Perk Database")]
public class PerkDatabase : ScriptableObject
{
	private static PerkDatabase _instance;
	public static PerkDatabase Instance
	{
		get
		{
			if (_instance == null)
				_instance = Resources.Load<PerkDatabase>("PerkDatabase");
			return _instance;
		}
	}

	[SerializeField] private List<Perk> allPerks = new List<Perk>();
	private Dictionary<string, Perk> perkDictionary;

	private void OnEnable()
	{
		if (_instance == null)
			_instance = this;
		CachePerks();
	}

	private void CachePerks()
	{
		perkDictionary = new Dictionary<string, Perk>();
		foreach (var perk in allPerks)
		{
			perkDictionary[perk.Name] = perk;
		}
	}

	public Perk GetPerkByName(string name)
	{
		return perkDictionary.TryGetValue(name, out var perk) ? perk : null;
	}

	public List<Perk> GetAllPerks() => allPerks;
}
