using System.Collections;
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
	private PerkTracker perkTracker;

	private void Start()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChange += UpdateStatDisplay;
		playerEvents.onGainExperience += UpdateExperience;
		playerEvents.onHealthChange += UpdateHealth;
		playerEvents.onArmorChange += UpdateArmor;
		GameEventsManager.instance.goldEvents.onGoldGained += UpdateGold;

		playerStats = PlayerStatsManager.instance.PlayerStats;
		playerStatsManager = PlayerStatsManager.instance;

		perkTracker = new PerkTracker(playerEvents);

		InitializeStatsUI();
		
		StartCoroutine(InitializeUIWithDelay());
	}

	private IEnumerator InitializeUIWithDelay()
	{
		yield return null;

		if (PerkDatabase.Instance != null)
		{
			// Kill a wolf to get perk
			#if UNITY_EDITOR
			GameEventsManager.instance.playerEvents.EnemyKilled("Wolf");
#endif

			RefreshUI();
		}
		else
		{
			Debug.LogError("PerkDatabase.Instance is null! Make sure it's properly initialized.");
		}
	}

	private void OnDestroy()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		playerEvents.onStatChange -= UpdateStatDisplay;
		playerEvents.onGainExperience -= UpdateExperience;
		playerEvents.onHealthChange -= UpdateHealth;
		playerEvents.onArmorChange -= UpdateArmor;
		GameEventsManager.instance.goldEvents.onGoldGained -= UpdateGold;

		perkTracker.Cleanup();
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
        armorText.text = $"{newArmorValue}";
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
		if (PerkDatabase.Instance == null || PerkDatabase.Instance.AllPerks == null)
		{
			Debug.LogWarning("PerkDatabase not ready yet");
			return;
		}

		foreach (var perkObj in displayedPerks)
		{
			Destroy(perkObj);
		}
		displayedPerks.Clear();

		foreach (Perk perk in PerkDatabase.Instance.AllPerks)
		{
		    bool isUnlocked = perk.CanUnlock(playerStats);
		    GameObject perkEntry = Instantiate(perkPrefab, perkContainer);

		    perkEntry.GetComponent<PerkDisplayPrefab>().Setup(perk, isUnlocked);
		    displayedPerks.Add(perkEntry);

		    // Add Tooltip Handler
		    PerkTooltipHandler tooltipHandler = perkEntry.AddComponent<PerkTooltipHandler>();
		    tooltipHandler.Initialize(perkTooltip, perk, isUnlocked);
		}
	}
}
