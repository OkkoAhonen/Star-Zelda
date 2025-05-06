using System;
using UnityEngine;

public class DerivedStats
{
	private PlayerStatsManager playerStatsManager;

	public int MaxHP { get; private set; }
	public int Stamina { get; private set; }
	public int MeleeDamage { get; private set; }
	public int MagicDamage { get; private set; }
	public int ArmorRating { get; private set; }
	public float CriticalHitChance { get; private set; }
	public float DodgeChance { get; private set; }
	public int MagicDefense { get; private set; }

	public DerivedStats(PlayerStatsManager statsManager)
	{
		playerStatsManager = statsManager;
		RecalculateStats();

		playerStatsManager.onStatChanged += RecalculateStats;
	}

	private void RecalculateStats()
	{
		MaxHP = playerStatsManager.GetStat(StatType.Vitality) * 10 +
				playerStatsManager.GetStat(StatType.Endurance) * 5;

		Stamina = playerStatsManager.GetStat(StatType.Endurance) * 8;

		MeleeDamage = playerStatsManager.GetStat(StatType.Strength) * 2;

		MagicDamage = playerStatsManager.GetStat(StatType.Magic) * 2 +
					  playerStatsManager.GetStat(StatType.Intelligence) * 1;

		ArmorRating = playerStatsManager.GetStat(StatType.Endurance) * 2 +
					  playerStatsManager.GetStat(StatType.Strength) * 1;

		CriticalHitChance = playerStatsManager.GetStat(StatType.Luck) * 0.5f;

		DodgeChance = playerStatsManager.GetStat(StatType.Agility) * 0.4f;
	}
}
