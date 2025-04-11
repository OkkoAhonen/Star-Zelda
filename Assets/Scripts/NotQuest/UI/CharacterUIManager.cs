using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
	[Header("Main Stats UI")]
	[SerializeField] private TMP_Text healthNumber;
	[SerializeField] private TMP_Text armorNumber;
	[SerializeField] private TMP_Text experienceNumber;
	[SerializeField] private TMP_Text goldNumber;

	[Header("Stats UI")]
	[SerializeField] private Transform statContainer;
	[SerializeField] private GameObject statPrefab;

	private Dictionary<StatType, TMP_Text> statTexts = new Dictionary<StatType, TMP_Text>();
	private List<GameObject> displayedPerks = new List<GameObject>();

	private PlayerStatsManager playerStatsManager;
	private PerkTracker perkTracker;

	private void Start()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChange += UpdateStatDisplay;
		playerEvents.onGainExperience += UpdateExperience;
		playerEvents.onHealthChangeTo += UpdateHealth;
		playerEvents.onChangeArmorBy += UpdateArmor;
		GameEventsManager.instance.playerEvents.onChangeGoldTo += UpdateGold;

		playerStatsManager = PlayerStatsManager.instance;

		perkTracker = new PerkTracker(playerEvents);

		InitializeStatsUI();
		RefreshUI();
	}

	private void InitializeStatsUI()
	{
		foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
		{
			GameObject statEntry = Instantiate(statPrefab, statContainer);
			TMP_Text statText = statEntry.transform.GetChild(1).GetComponent<TMP_Text>();
			statText.text = stat.ToString() + ": 0";
			statTexts[stat] = statText;
		}
	}

	private void RefreshUI()
	{
		healthNumber.text = $"{playerStatsManager.CurrentHealth}/{playerStatsManager.MaxHealth}";
		armorNumber.text = $"{playerStatsManager.CurrentArmor}";
		experienceNumber.text = $"{playerStatsManager.CurrentExperience}/{playerStatsManager.XPToNextLevel}";
		goldNumber.text = $"{playerStatsManager.CurrentGold}";

		foreach (var stat in statTexts)
		{
			stat.Value.text = $"{stat.Key}: {playerStatsManager.GetStat(stat.Key)}";
		}
	}

	private void UpdateStatDisplay(StatType statType, int newValue)
	{
		if (statTexts.TryGetValue(statType, out var text))
		{
			text.text = $"{statType}: {newValue}";
		}
	}

	private void UpdateHealth(int currentHealth, int maxHealth)
	{
		healthNumber.text = $"{currentHealth}/{maxHealth}";
	}

	private void UpdateExperience(int newXP)
	{
		experienceNumber.text = $"{playerStatsManager.CurrentExperience}/{playerStatsManager.XPToNextLevel}";
	}

	private void UpdateGold(int temp)
	{
		goldNumber.text = $"{playerStatsManager.CurrentGold}";
	}

	private void UpdateArmor(int newArmorValue)
	{
        armorNumber.text = $"{newArmorValue}";
	}
}
