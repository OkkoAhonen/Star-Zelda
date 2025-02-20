using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
	Strength, Dexterity, Vitality, Intelligence, Sanity, Wisdom, Charisma,
	Endurance, Agility, Luck, Perception, Willpower,
	Magic, Faith, Alchemy, Ranged, Instinct, Armor
}

public class PlayerStats
{
	private Dictionary<StatType, int> stats = new Dictionary<StatType, int>();
	private Dictionary<StatType, List<int>> statUnlockThresholds = new Dictionary<StatType, List<int>>();

	public int CurrentHealth { get; private set; }
	public int MaxHealth => stats[StatType.Vitality] * 10;

	public PlayerStats()
	{
		foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
		{
			stats[stat] = 1;
			statUnlockThresholds[stat] = new List<int>();
		}

		// Example Unlocks
		statUnlockThresholds[StatType.Strength].AddRange(new int[] { 3, 6, 10 });
		statUnlockThresholds[StatType.Intelligence].AddRange(new int[] { 2, 5, 8 });
		statUnlockThresholds[StatType.Charisma].AddRange(new int[] { 4, 7, 10 });

		CurrentHealth = MaxHealth;
	}

	public int GetStat(StatType stat) => stats[stat];

	public void IncreaseStat(StatType stat)
	{
		stats[stat]++;
		Debug.Log(stat + " increased to " + stats[stat]);
	}

	public bool CanUnlockStat(StatType stat)
	{
		return statUnlockThresholds[stat].Contains(stats[stat]);
	}

	public void TakeDamage(int amount)
	{
		CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
		Debug.Log("Player took " + amount + " damage. Health: " + CurrentHealth + "/" + MaxHealth);
	}

	public void Heal(int amount)
	{
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
		Debug.Log("Player healed " + amount + " HP. Health: " + CurrentHealth + "/" + MaxHealth);
	}
}
