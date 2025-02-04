using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int startingExperience = 0;
    [SerializeField] private int startingGold = 5;

    public int currentLevel { get; private set; }
    public int currentExperience { get; private set; }
    public int currentGold { get; private set; }

    private void Awake()
    {
        currentLevel = startingLevel;
        currentExperience = startingExperience;
        currentGold = startingGold;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.playerEvents.onExperienceGained += ExperienceGained;
        GameEventsManager.instance.goldEvents.onGoldGained += GoldGained;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.playerEvents.onExperienceGained -= ExperienceGained;
        GameEventsManager.instance.goldEvents.onGoldGained -= GoldGained;
    }

    private void Start()
    {
        GameEventsManager.instance.playerEvents.PlayerLevelChange(currentLevel);
        GameEventsManager.instance.playerEvents.PlayerExperienceChange(currentExperience);
        GameEventsManager.instance.goldEvents.GoldChange(currentGold);
    }

    private void ExperienceGained(int experience)
    {
        currentExperience += experience;
        // check if we're ready to level up
        while (currentExperience >= GlobalConstants.experienceToLevelUp)
        {
            currentExperience -= GlobalConstants.experienceToLevelUp;
            currentLevel++;
            GameEventsManager.instance.playerEvents.PlayerLevelChange(currentLevel);
        }
        GameEventsManager.instance.playerEvents.PlayerExperienceChange(currentExperience);
    }

    private void GoldGained(int gold)
    {
        currentGold += gold;
        GameEventsManager.instance.goldEvents.GoldChange(currentGold);
    }
}