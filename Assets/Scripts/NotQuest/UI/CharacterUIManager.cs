using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{ // This script is for all the UI concerning perks, stats, attributes and so on
	[Header("Stats UI")]
	[SerializeField] private Transform statContainer;
	[SerializeField] private GameObject statPrefab;

	[Header("Stat Categories")]
	[SerializeField] private Transform bodyStatContainer;
	[SerializeField] private Transform accuracyStatContainer;
	[SerializeField] private Transform magicPowerStatContainer;

	[Header("Remaining Points")]
	[SerializeField] private TMP_Text bodyPointsText;
	[SerializeField] private TMP_Text accuracyPointsText;
	[SerializeField] private TMP_Text magicPointsText;

	[Header("Attributes UI")]
	[SerializeField] private TMP_Text attributePointsText;

	[SerializeField] private TMP_Text bodyAttributeText;
	[SerializeField] private TMP_Text accuracyAttributeText;
	[SerializeField] private TMP_Text magicAttributeText;

	[SerializeField] private Button bodyAttributeButton;
	[SerializeField] private Button accuracyAttributeButton;
	[SerializeField] private Button magicAttributeButton;


	public static CharacterUIManager instance { get; private set; }

	private Dictionary<StatType, TMP_Text> statTexts = new Dictionary<StatType, TMP_Text>();
	private List<GameObject> displayedPerks = new List<GameObject>();

	private PlayerStatsManager playerStatsManager;
	private Dictionary<StatType, Button> buttonByStat = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicate managers
            return;
        }
        instance = this;
    }	
	
	private void Start()
	{
		var playerEvents = GameEventsManager.instance.playerEvents;
		//playerEvents.onStatChange += UpdateStatDisplay;

		playerStatsManager = PlayerStatsManager.instance;

		InitializeStatsUI();

		bodyAttributeButton.onClick.AddListener(() =>
		{
			playerStatsManager.SpendAttributePoint("body");
			RefreshUI();
		});
		accuracyAttributeButton.onClick.AddListener(() =>
		{
			playerStatsManager.SpendAttributePoint("accuracy");
			RefreshUI();
		});
		magicAttributeButton.onClick.AddListener(() =>
		{
			playerStatsManager.SpendAttributePoint("magicpowers");
			RefreshUI();
		});

		RefreshUI();
	}

	private void InitializeStatsUI()
	{
		foreach (StatType stat in Enum.GetValues(typeof(StatType)))
		{
			GameObject statEntry = Instantiate(statPrefab);

			TMP_Text statNameText = statEntry.transform.GetChild(0).GetComponent<TMP_Text>();
			TMP_Text statValueText = statEntry.transform.GetChild(1).GetComponent<TMP_Text>();
			Button increaseButton = statEntry.transform.GetChild(2).GetComponent<Button>();

			statNameText.text = stat.ToString();
			statValueText.text = $"{playerStatsManager.GetStat(stat)}";

			StatType capturedStat = stat;
			increaseButton.onClick.AddListener(() =>
			{
				playerStatsManager.SpendLevelPointOnStat(capturedStat);
				RefreshUI(); // Update everything after the change
			});

			statTexts[stat] = statValueText;

			// Determine where to place the stat in the UI
			if (playerStatsManager.IsBodyStat(stat))
				statEntry.transform.SetParent(bodyStatContainer, false);
			else if (playerStatsManager.IsAccuracyStat(stat))
				statEntry.transform.SetParent(accuracyStatContainer, false);
			else if (playerStatsManager.IsMagicStat(stat))
				statEntry.transform.SetParent(magicPowerStatContainer, false);
			else
				statEntry.transform.SetParent(statContainer, false); // fallback

			// Store reference to the button to enable/disable it later
			buttonByStat[stat] = increaseButton;
		}
	}

	public void RefreshUI()
	{
		// Update remaining points
		bodyPointsText.text = $"{playerStatsManager.GetRemainingPoints("body")}";
		accuracyPointsText.text = $"{playerStatsManager.GetRemainingPoints("accuracy")}";
		magicPointsText.text = $"{playerStatsManager.GetRemainingPoints("magicpowers")}";
		
		// Show how many attribute points remain
		attributePointsText.text = $"{playerStatsManager.GetRemainingAttributePoints()}";

		// Update attribute values
		bodyAttributeText.text = playerStatsManager.GetAttribute("body").ToString();
		accuracyAttributeText.text = playerStatsManager.GetAttribute("accuracy").ToString();
		magicAttributeText.text = playerStatsManager.GetAttribute("magicpowers").ToString();

		// Enable/disable attribute buttons
		bool canSpend = playerStatsManager.GetRemainingAttributePoints() > 0;
		bodyAttributeButton.interactable = canSpend;
		accuracyAttributeButton.interactable = canSpend;
		magicAttributeButton.interactable = canSpend;


		foreach (var stat in statTexts)
		{
			stat.Value.text = $"{playerStatsManager.GetStat(stat.Key)}";

			// Enable/disable buttons based on available points
			Button btn = buttonByStat[stat.Key];
			bool enabled = false;

			if (playerStatsManager.IsBodyStat(stat.Key))
				enabled = playerStatsManager.GetRemainingPoints("body") > 0;
			else if (playerStatsManager.IsAccuracyStat(stat.Key))
				enabled = playerStatsManager.GetRemainingPoints("accuracy") > 0;
			else if (playerStatsManager.IsMagicStat(stat.Key))
				enabled = playerStatsManager.GetRemainingPoints("magicpowers") > 0;

			btn.interactable = enabled;
		}
	}


	private void UpdateStatDisplay(StatType statType, int newValue)
	{
		if (statTexts.TryGetValue(statType, out var text))
		{
			text.text = $"{statType}: {newValue}";
		}
	}
}
