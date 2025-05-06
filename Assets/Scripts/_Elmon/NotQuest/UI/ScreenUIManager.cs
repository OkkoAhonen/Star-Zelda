using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScreenUIManager : MonoBehaviour
{ // This script is for all the UI on screen like health, gold and levels. Everything here updates based on events, meaning it's super efficient
    public static ScreenUIManager instance { get; private set; }

    [SerializeField] private TMP_Text healthNumber;
    [SerializeField] private TMP_Text experienceNumber;
    [SerializeField] private TMP_Text goldNumber;
    [SerializeField] private TMP_Text armorNumber;
    [SerializeField] private TMP_Text levelNumber;
    [SerializeField] private Slider levelSlider;


    private PlayerStatsManager playerStatsManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        playerStatsManager = PlayerStatsManager.instance;

        var playerEvents = GameEventsManager.instance.playerEvents;
        playerEvents.onHealthChangeTo += UpdateHealth;
        playerEvents.onChangeArmorBy += UpdateArmor;
        playerEvents.onGainExperience += UpdateExperience;
        playerEvents.onChangeGoldBy += UpdateGold;

        RefreshScreenUI();
    }

    public void RefreshScreenUI()
    {
        UpdateHealth(playerStatsManager.CurrentHealth, playerStatsManager.MaxHealth);
        UpdateArmor(playerStatsManager.CurrentArmor);
        UpdateExperience(playerStatsManager.CurrentExperience);
        UpdateGold(playerStatsManager.CurrentGold);
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthNumber.text = $"{currentHealth}/{maxHealth}";
    }

    private void UpdateArmor(int newArmorValue)
    {
        armorNumber.text = $"{newArmorValue}";
    }

    private void UpdateExperience(float xp)
    {
        float currentXP = playerStatsManager.CurrentExperience;
        float requiredXP = playerStatsManager.XPToNextLevel;
        
        levelNumber.text = $"{playerStatsManager.CurrentLevel}";
        levelSlider.value = currentXP / requiredXP;
        experienceNumber.text = $"{currentXP}/{playerStatsManager.XPToNextLevel}";
    }

    private void UpdateGold(int gold)
    {
        goldNumber.text = $"{playerStatsManager.CurrentGold}";
    }
}
