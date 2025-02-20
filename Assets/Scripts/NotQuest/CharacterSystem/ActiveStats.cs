using System.Collections.Generic;
using UnityEngine;

public class ActiveStats
{
    private PlayerStats baseStats;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => baseStats.GetStat(StatType.Vitality) * 10;
    public int CurrentArmor { get; private set; }
    public int CurrentSanity { get; private set; }
    public int MaxSanity => baseStats.GetStat(StatType.Willpower) * 5;
    public int CurrentExperience { get; private set; }
    public int CurrentGold { get; private set; }

    public ActiveStats(PlayerStats baseStats, int startingGold = 0)
    {
        this.baseStats = baseStats;
        CurrentHealth = MaxHealth;
        CurrentSanity = MaxSanity;
        CurrentGold = startingGold;
        CurrentArmor = 0;
        CurrentExperience = 0;
    }

    public void ModifyHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
    }

    public void ModifyArmor(int amount)
    {
        CurrentArmor = Mathf.Max(0, CurrentArmor + amount);
    }

    public void ModifySanity(int amount)
    {
        CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0, MaxSanity);
    }

    public void AddExperience(int amount)
    {
        CurrentExperience += amount;
    }

    public void ModifyGold(int amount)
    {
        CurrentGold = Mathf.Max(0, CurrentGold + amount);
    }
} 