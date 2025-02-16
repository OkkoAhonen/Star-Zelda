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
	[SerializeField] private PerkTooltip perkTooltip;

	private Dictionary<StatType, TMP_Text> statTexts = new Dictionary<StatType, TMP_Text>();
	private List<GameObject> displayedPerks = new List<GameObject>();

	private PlayerStats playerStats;
	private PlayerStatsManager playerStatsManager;

	private void Start()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChanged += UpdateStatDisplay;
		playerEvents.onGainExperience += UpdateExperience;
		playerEvents.onHealthChanged += UpdateHealth;
		playerEvents.onArmorChanged += UpdateArmor;
		GameEventsManager.instance.goldEvents.onGoldGained += UpdateGold;

		playerStats = PlayerStatsManager.instance.PlayerStats;
		playerStatsManager = PlayerStatsManager.instance;

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
		healthText.text = $"Health: {playerStats.CurrentHealth}/{playerStats.MaxHealth}";
		armorText.text = $"Armor: {playerStats.GetStat(StatType.Armor)}";
		experienceText.text = $"XP: {playerStatsManager.CurrentExperience}";
		goldText.text = $"Gold: {playerStatsManager.CurrentGold}";

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

		foreach (var perk in PerkDatabase.Instance.GetAllPerks())
		{
			bool isUnlocked = perk.CanUnlock(playerStats);
			GameObject perkEntry = Instantiate(perkPrefab, perkContainer);

			TMP_Text nameText = perkEntry.transform.Find("Name").GetComponent<TMP_Text>();
			TMP_Text descriptionText = perkEntry.transform.Find("Description").GetComponent<TMP_Text>();
			Image icon = perkEntry.transform.Find("Icon").GetComponent<Image>();

			nameText.text = perk.Name;
			descriptionText.text = perk.Description;
			icon.sprite = perk.Icon;

			// Gray out locked perks
			icon.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
			displayedPerks.Add(perkEntry);

			// Add Tooltip Event Listeners
			PerkTooltipHandler tooltipHandler = perkEntry.AddComponent<PerkTooltipHandler>();
			tooltipHandler.Initialize(perkTooltip, perk, isUnlocked);
		}
	}
}
