using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private int startingLevel = 1;
	[SerializeField] private int startingExperience = 0;
	[SerializeField] private int startingGold = 5;
	[SerializeField] private int startingArmor = 0;

	public int CurrentLevel { get; private set; }
	public int CurrentExperience { get; private set; }
	public int CurrentGold { get; private set; }
	public int CurrentArmor { get; private set; }

	private void Awake()
	{
		CurrentLevel = startingLevel;
		CurrentExperience = startingExperience;
		CurrentGold = startingGold;
		CurrentArmor = startingArmor;
	}

	private void OnEnable()
	{
		GameEventsManager.instance.playerEvents.onGainExperience += GainExperience;
		GameEventsManager.instance.goldEvents.onGoldGained += GainGold;
	}

	private void OnDisable()
	{
		GameEventsManager.instance.playerEvents.onGainExperience -= GainExperience;
		GameEventsManager.instance.goldEvents.onGoldGained -= GainGold;
	}

	private void Start()
	{
		GameEventsManager.instance.playerEvents.PlayerLevelChange(CurrentLevel);
		GameEventsManager.instance.playerEvents.PlayerExperienceChange(CurrentExperience);
		GameEventsManager.instance.goldEvents.GoldChange(CurrentGold);
		GameEventsManager.instance.playerEvents.PlayerArmorChange(CurrentArmor);
	}

	private void GainExperience(int experience)
	{
		CurrentExperience += experience;

		while (CurrentExperience >= GlobalConstants.experienceToLevelUp)
		{
			CurrentExperience -= GlobalConstants.experienceToLevelUp;
			CurrentLevel++;
			GameEventsManager.instance.playerEvents.PlayerLevelChange(CurrentLevel);
		}

		GameEventsManager.instance.playerEvents.PlayerExperienceChange(CurrentExperience);
	}

	private void GainGold(int gold)
	{
		CurrentGold += gold;
		GameEventsManager.instance.goldEvents.GoldChange(CurrentGold);
	}

	public void ChangeArmor(int amount)
	{
		CurrentArmor = Mathf.Max(0, CurrentArmor + amount);
		GameEventsManager.instance.playerEvents.PlayerArmorChange(CurrentArmor);
	}
}
