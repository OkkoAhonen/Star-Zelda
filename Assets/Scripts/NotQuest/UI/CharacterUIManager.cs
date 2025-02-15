using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
	[Header("Main Stats UI")]
	[SerializeField] private TMP_Text healthText;
	[SerializeField] private TMP_Text armorText;
	[SerializeField] private TMP_Text experienceText;
	[SerializeField] private TMP_Text goldText;

	[Header("Stats UI")]
	[SerializeField] private Transform statContainer;
	[SerializeField] private GameObject statPrefab;

	[Header("Perks UI")]
	[SerializeField] private Transform perkContainer;
	[SerializeField] private GameObject perkPrefab;

	private Dictionary<StatType, TMP_Text> statTexts = new Dictionary<StatType, TMP_Text>();
	private List<GameObject> displayedPerks = new List<GameObject>();

	private void Start()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChanged += UpdateStatDisplay;
		playerEvents.onGainExperience += UpdateExperience;
		playerEvents.onHealthChanged += UpdateHealth;
		playerEvents.onArmorChanged += UpdateArmor;
		GameEventsManager.instance.goldEvents.onGoldGained += UpdateGold;

		InitializeStatsUI();
		RefreshUI();
	}

	private void OnDestroy()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChanged -= UpdateStatDisplay;
		playerEvents.onGainExperience -= UpdateExperience;
		playerEvents.onHealthChanged -= UpdateHealth;
		playerEvents.onArmorChanged -= UpdateArmor;
		GameEventsManager.instance.goldEvents.onGoldGained -= UpdateGold;
	}

	private void InitializeStatsUI()
	{
		foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
		{
			GameObject statEntry = Instantiate(statPrefab, statContainer);
			TMP_Text statText = statEntry.GetComponent<TMP_Text>();
			statText.text = stat.ToString() + ": 0";
			statTexts[stat] = statText;
		}
	}

	private void RefreshUI()
	{
		var playerStats = GameManager.instance.PlayerStats;

		healthText.text = $"Health: {playerStats.CurrentHealth}/{playerStats.MaxHealth}";
		armorText.text = $"Armor: {playerStats.GetStat(StatType.Armor)}";
		experienceText.text = $"XP: {GameManager.instance.PlayerStatsManager.CurrentExperience}";
		goldText.text = $"Gold: {GameManager.instance.PlayerStatsManager.CurrentGold}";

		foreach (var stat in statTexts)
		{
			stat.Value.text = $"{stat.Key}: {playerStats.GetStat(stat.Key)}";
		}

		RefreshPerksUI();
	}

	private void UpdateStatDisplay(StatType statType, int newValue)
	{
		if (statTexts.TryGetValue(statType, out var text))
		{
			text.text = $"{statType}: {newValue}";
		}
		RefreshPerksUI();
	}

	private void UpdateHealth(int currentHealth, int maxHealth)
	{
		healthText.text = $"Health: {currentHealth}/{maxHealth}";
	}

	private void UpdateArmor(int newArmorValue)
	{
		armorText.text = $"Armor: {newArmorValue}";
	}

	private void UpdateExperience(int newXP)
	{
		experienceText.text = $"XP: {newXP}";
	}

	private void UpdateGold(int newGold)
	{
		goldText.text = $"Gold: {newGold}";
	}

	private void RefreshPerksUI()
	{
		foreach (var perkObj in displayedPerks)
		{
			Destroy(perkObj);
		}
		displayedPerks.Clear();

		var playerStats = GameManager.instance.PlayerStats;
		foreach (var perk in PerkDatabase.Instance.GetAllPerks())
		{
			if (perk.CanUnlock(playerStats))
			{
				GameObject perkEntry = Instantiate(perkPrefab, perkContainer);
				perkEntry.transform.Find("Name").GetComponent<TMP_Text>().text = perk.Name;
				perkEntry.transform.Find("Description").GetComponent<TMP_Text>().text = perk.Description;
				perkEntry.transform.Find("Icon").GetComponent<Image>().sprite = perk.Icon;
				displayedPerks.Add(perkEntry);
			}
		}
	}
}
