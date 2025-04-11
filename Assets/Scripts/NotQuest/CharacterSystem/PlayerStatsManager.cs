using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public enum StatType
{
	Strength, Dexterity, Vitality, Intelligence, Sanity, Wisdom, Charisma,
	Endurance, Agility, Luck, Perception, Willpower,
	Magic, Faith, Alchemy, Ranged, Instinct
}

public class PlayerStatsManager : MonoBehaviour
{
	private readonly HashSet<StatType> bodyStats = new HashSet<StatType>
	{
		StatType.Strength, StatType.Vitality, StatType.Endurance
	};

	private readonly HashSet<StatType> accuracyStats = new HashSet<StatType>
	{
		StatType.Agility, StatType.Luck, StatType.Dexterity, StatType.Perception, StatType.Ranged
	};

	private readonly HashSet<StatType> magicPowerStats = new HashSet<StatType>
	{
		StatType.Magic, StatType.Faith, StatType.Alchemy, StatType.Intelligence, StatType.Wisdom, StatType.Willpower
	};

	public static PlayerStatsManager instance { get; private set; }
	public event Action onStatChanged;
	
	private Dictionary<StatType, int> stats = new Dictionary<StatType, int>();

	[Header("Configuration")]
	[SerializeField] private int startingLevel = 1;
	[SerializeField] private int startingExperience = 0;
	[SerializeField] private int startingGold = 98;
	[SerializeField] private int startingArmor = 0;

	// { get; private set; }  eli voi ottaa, mutta ei muutta muista scripteistä
	public int CurrentLevel { get; private set; }
	public int CurrentExperience { get; private set; }
	public int CurrentGold { get; private set; }
	public int CurrentArmor { get; private set; }
	public int CurrentHealth { get; private set; }
	public int MaxHealth { get; private set; }
	public int XPToNextLevel { get; private set; }
	public int ExperienceNeededPerLevel { get; private set; }
	public int PointsPerLevel { get; private set; }

	// Main attributes are integers (for now)
	public int Body { get; private set; }
	public int Accuracy { get; private set; }
	public int MagicPowers { get; private set; }

	//private int levelPoints; // Spend on skills
	private int bodyLevelPoints;
	private int accuracyLevelPoints;
	private int magicPowersLevelPoints;
	private int attributePoints; // Spend on attributes
	private int perkPurchasesAvailable;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		
		// Initialize the stats dictionary
		stats = new Dictionary<StatType, int>();

		foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
		{
			stats[stat] = 1; // Initialize all stats to 1 (needed?)
		}

		MaxHealth = 50 + GetStat(StatType.Vitality) * 5;
		ExperienceNeededPerLevel = 100;
		CurrentLevel = startingLevel;
		CurrentExperience = startingExperience;
		CurrentGold = startingGold;
		CurrentArmor = startingArmor;
		CurrentHealth = MaxHealth;
		XPToNextLevel = ExperienceNeededPerLevel;
	}

	public int GetStat(StatType stat) => stats[stat];

	public void IncreaseStat(StatType stat, int amount)
	{ // increase stat by amount
		stats[stat] += amount;
		Debug.Log(stat + " increased to " + stats[stat]);
		onStatChanged?.Invoke();
	}

	public void SpendAttributePoint(string attribute)
    {
        if (attributePoints <= 0)
        {
            Debug.LogWarning("No attribute points.");
            return;
        }
        attributePoints--;
        switch (attribute.ToLower())
        {
            case "body":
                bodyLevelPoints += PointsPerLevel;
                break;
            case "accuracy":
                accuracyLevelPoints += PointsPerLevel;
                break;
            case "magicpowers":
                magicPowersLevelPoints += PointsPerLevel;
                break;
            default:
                Debug.LogWarning("Unknown attribute.");
                break;
        }
    }

	public int GetAttribute(string attribute)
    {
        switch (attribute.ToLower())
        {
            case "body":
                return Body;
            case "accuracy":
                return Accuracy;
            case "magicpowers":
                return MagicPowers;
            default:
                return 0;
        }
    }

	public void SpendLevelPointOnStat(StatType stat)
	{
		if (bodyStats.Contains(stat))
		{
			if (bodyLevelPoints <= 0)
			{
				Debug.LogWarning("No Body level points remaining.");
				return;
			}

			stats[stat] += 1;
			bodyLevelPoints--;
			Debug.Log($"Increased {stat} by 1. Remaining Body points: {bodyLevelPoints}");
		}
		else if (accuracyStats.Contains(stat))
		{
			if (accuracyLevelPoints <= 0)
			{
				Debug.LogWarning("No Accuracy level points remaining.");
				return;
			}

			stats[stat] += 1;
			accuracyLevelPoints--;
			Debug.Log($"Increased {stat} by 1. Remaining Accuracy points: {accuracyLevelPoints}");
		}
		else if (magicPowerStats.Contains(stat))
		{
			if (magicPowersLevelPoints <= 0)
			{
				Debug.LogWarning("No Magic Power level points remaining.");
				return;
			}

			stats[stat] += 1;
			magicPowersLevelPoints--;
			Debug.Log($"Increased {stat} by 1. Remaining MagicPower points: {magicPowersLevelPoints}");
		}
		else
		{
			Debug.LogWarning($"Stat {stat} does not belong to any known category.");
		}

		onStatChanged?.Invoke();
	}

	private void GainExperience(int experience)
	{
		CurrentExperience += experience;

		while (CurrentExperience >= XPToNextLevel)
		{ // "while" for if you get more than one level's worth of xp 
			CurrentExperience -= XPToNextLevel;
			CurrentLevel++;
			attributePoints++;
			if (CurrentLevel % 3 == 0) // Every 3 levels
			{
				perkPurchasesAvailable++;
			}

			GameEventsManager.instance.playerEvents.PlayerLevelChangeTo(CurrentLevel);
		}
	}

	public void SpendAttributePoints(StatType attribute, int points)
	{
		if (attributePoints >= points)
		{
			stats[attribute] += points * 15; // Each point gives 15 to the attribute
			attributePoints -= points;
			Debug.Log($"{attribute} increased by {points * 15}.");
			onStatChanged?.Invoke();
		}
		else
		{
			Debug.Log("Not enough attribute points.");
		}
	}

	private void ChangeGold(int gold)
	{
		CurrentGold += gold;
	}

	public void ChangeArmor(int amount)
	{
		CurrentArmor = Mathf.Max(0, CurrentArmor + amount);
		GameEventsManager.instance.playerEvents.ChangeArmorBy(CurrentArmor);
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

	private void OnEnable()
	{
		GameEventsManager.instance.playerEvents.onGainExperience += GainExperience;
		GameEventsManager.instance.playerEvents.onChangeGoldTo += ChangeGold;
	}

	private void OnDisable()
	{
		GameEventsManager.instance.playerEvents.onGainExperience -= GainExperience;
		GameEventsManager.instance.playerEvents.onChangeGoldTo -= ChangeGold;
	}
}
