using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public enum StatType
{
	Strength, // Melee 
	Vitality, // Max health
	Intelligence, // More levels, stronger magic
	Endurance, // Stamina?
	Agility, // Speed
	Luck, // Luck
	Magic, // Magic
	Alchemy, // Potions
	Ranged // Bow
}

public class PlayerStatsManager : MonoBehaviour
{


    private readonly HashSet<StatType> bodyStats = new HashSet<StatType>
	{
		StatType.Strength, StatType.Vitality, StatType.Endurance
	};

	private readonly HashSet<StatType> accuracyStats = new HashSet<StatType>
	{
		StatType.Agility, StatType.Luck, StatType.Ranged
	};

	private readonly HashSet<StatType> magicPowerStats = new HashSet<StatType>
	{
        StatType.Magic, StatType.Alchemy, StatType.Intelligence
	};

	public static PlayerStatsManager instance { get; private set; }
	public event Action onStatChanged;
	
	public Dictionary<StatType, int> stats = new Dictionary<StatType, int>();

	[Header("Configuration")]
	[SerializeField] private int startingLevel = 1;
	[SerializeField] private int startingExperience = 0;
	[SerializeField] private int startingGold = 98;
	[SerializeField] private int startingArmor = 0;
	[SerializeField] private int startingAttributePoints = 5;

	// These values are read-only from other scripts
	public int CurrentLevel { get; private set; }
	public float CurrentExperience { get; private set; }
	public int CurrentGold { get; private set; }
	public int CurrentArmor { get; private set; }
	public int CurrentHealth { get; private set; }
	public int MaxHealth { get; private set; }
	public int XPToNextLevel { get; private set; }
	public int ExperienceNeededPerLevel { get; private set; }
	public int PointsPerLevel { get; private set; }
	public int perkPurchasesAvailable { get; private set; }

	// Main attributes (calculated as the sum of stats from each group, if desired)
	public int Body { get; private set; }
	public int Accuracy { get; private set; }
	public int MagicPowers { get; private set; }

	// Level point pools for spending on stats under attributes
	private int bodyLevelPoints;
	private int accuracyLevelPoints;
	private int magicPowersLevelPoints;
	private int attributePoints; // Spend on attributes


	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;

		PointsPerLevel = 15;
		
		// Initialize attribute points with starting value
		attributePoints = startingAttributePoints;
		
		// Initialize the stats dictionary
		stats = new Dictionary<StatType, int>();
		foreach (StatType stat in Enum.GetValues(typeof(StatType)))
		{
			stats[stat] = 1;
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
	{
		stats[stat] += amount;
		Debug.Log(stat + " increased to " + stats[stat]);
		onStatChanged?.Invoke();
	}

    private void Update()
    {
		if(CurrentHealth <= 10) { 
		CurrentHealth += 1;
        }
    }

    public int GetRemainingPoints(string category)
	{
		switch (category.ToLower())
		{
			case "body": return bodyLevelPoints;
			case "accuracy": return accuracyLevelPoints;
			case "magicpowers": return magicPowersLevelPoints;
			default: return 0;
		}
	}

	public bool IsBodyStat(StatType stat) => bodyStats.Contains(stat);
	public bool IsAccuracyStat(StatType stat) => accuracyStats.Contains(stat);
	public bool IsMagicStat(StatType stat) => magicPowerStats.Contains(stat);

	public int GetRemainingAttributePoints()
	{
		return attributePoints;
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
				Body++;
				bodyLevelPoints += PointsPerLevel;
				break;
			case "accuracy":
				Accuracy++;
				accuracyLevelPoints += PointsPerLevel;
				break;
			case "magicpowers":
				MagicPowers++;
				magicPowersLevelPoints += PointsPerLevel;
				break;
			default:
				Debug.LogWarning("Unknown attribute.");
				break;
		}
		onStatChanged?.Invoke(); // This will help if other things listen to it
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

	public void GainExperience(float experience)
	{
        CurrentExperience += experience * (1 + GetStat(StatType.Intelligence) * 0.1f);
		while (CurrentExperience >= XPToNextLevel)
		{
			CurrentExperience -= XPToNextLevel;
			CurrentLevel++;
			attributePoints++;
			if (CurrentLevel % 3 == 0)
			{
				perkPurchasesAvailable++;
			}
			Debug.Log("Level UP! Current level: " + CurrentLevel);
			GameEventsManager.instance.playerEvents.PlayerLevelChangeTo(CurrentLevel);
			CharacterUIManager.instance.RefreshUI();
		}
	}

	public void SpendAttributePoints(StatType attribute, int points)
	{
		if (attributePoints >= points)
		{
			stats[attribute] += points * 15;
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
        GameEventsManager.instance.playerEvents.HealthChangeTo(CurrentHealth, MaxHealth);
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
		GameEventsManager.instance.playerEvents.onChangeGoldBy += ChangeGold;
	}

	private void OnDisable()
	{
		GameEventsManager.instance.playerEvents.onGainExperience -= GainExperience;
		GameEventsManager.instance.playerEvents.onChangeGoldBy -= ChangeGold;
	}
}
