using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Central stats manager: Aspects -> Attributes -> Derived Stats
public class PlayerStatsManager : MonoBehaviour
{
    // Tier enums
    public enum AspectType { Body, Mind, Soul }
    public enum AttributeType
    {
        Strength,   // Melee damage
        Vitality,   // HP
        Agility,    // Movement / dodge
        Luck,       // Crit chance / gold find
        Magic,      // Spell damage / cast rate
        Alchemy,    // Potion power / throw range/speed
        Ranged      // Bow damage / range / fire rate
    }

    // Save data struct
    [Serializable]
    private class SaveData
    {
        public int CurrentLevel;
        public float CurrentExperience;
        public int CurrentGold;
        public int CurrentArmor;

        public int BodyAspectLevel;
        public int MindAspectLevel;
        public int SoulAspectLevel;

        public int BodyAspectPoints;
        public int MindAspectPoints;
        public int SoulAspectPoints;

        public int AttributePoints;

        public List<AttributeEntry> Attributes;

        public string EquippedHatID;
        public string EquippedShirtID;
        public string EquippedShoesID;
    }

    [Serializable]
    private struct AttributeEntry
    {
        public string AttributeName;
        public int Value;
    }

    // Singleton
    public static PlayerStatsManager instance { get; private set; }

    // Config
    [Header("Configuration")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private float startingExperience = 0f;
    [SerializeField] private int startingGold = 98;
    [SerializeField] private int startingArmor = 0;
    [Tooltip("How many aspect points are granted PER ASPECT on level up (keeps old behavior).")]
    [SerializeField] private int aspectPointsPerLevelPerAspect = 1;
    [Tooltip("How many attribute points each spent AspectPoint grants.")]
    [SerializeField] private int attributePointsPerAspectPoint = 3;
    [Tooltip("XP needed per level (linear).")]
    [SerializeField] private int experienceNeededPerLevel = 100;

    // Runtime progression
    public int CurrentLevel { get; private set; }
    public float CurrentExperience { get; private set; }
    public int CurrentGold { get; private set; }
    public int CurrentArmor { get; private set; }
    public int XPToNextLevel { get; private set; }

    // Aspect levels and per-aspect point pools
    public int BodyAspectLevel { get; private set; }
    public int MindAspectLevel { get; private set; }
    public int SoulAspectLevel { get; private set; }

    private int bodyAspectPoints;
    private int mindAspectPoints;
    private int soulAspectPoints;

    // Attribute points pool (global) granted when spending Aspect points
    private int attributePoints;

    // Tier 2: base attributes and equipment bonuses
    private Dictionary<AttributeType, int> baseAttributes = new Dictionary<AttributeType, int>();
    private Dictionary<AttributeType, int> equipmentAttributeBonuses = new Dictionary<AttributeType, int>();

    // Equipment items (hat/shirt/shoes)
    private Item equippedHat;
    private Item equippedShirt;
    private Item equippedShoes;

    // Tier 3: derived stats (floats internally)
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float MeleeDamage { get; private set; }
    public float MeleeSpeed { get; private set; }
    public float ThrowRange { get; private set; }
    public float ThrowSpeed { get; private set; }
    public float CastingRate { get; private set; }
    public float CriticalHitChance { get; private set; }
    public float DodgeChance { get; private set; }
    public float PotionEffectiveness { get; private set; }
    public float ArmorRating { get; private set; }
    public float GoldFindBonus { get; private set; } // extra gold multiplier

    // Save path
    private readonly string savePath = @"F:\GitHub\Star-Zelda\Assets\_SaveSystem\playerStats.json";

    // Aspect->Attributes mapping
    private readonly Dictionary<AspectType, HashSet<AttributeType>> aspectAttributes = new Dictionary<AspectType, HashSet<AttributeType>>()
    {
        { AspectType.Body, new HashSet<AttributeType>{ AttributeType.Strength, AttributeType.Vitality } },
        { AspectType.Mind, new HashSet<AttributeType>{ AttributeType.Agility, AttributeType.Luck, AttributeType.Ranged } },
        { AspectType.Soul, new HashSet<AttributeType>{ AttributeType.Magic, AttributeType.Alchemy } }
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // init attribute dictionaries
        foreach (AttributeType a in Enum.GetValues(typeof(AttributeType)))
        {
            baseAttributes[a] = 1;
            equipmentAttributeBonuses[a] = 0;
        }

        // try load; if fail, init defaults
        if (!LoadStats())
        {
            CurrentLevel = startingLevel;
            CurrentExperience = startingExperience;
            CurrentGold = startingGold;
            CurrentArmor = startingArmor;
            XPToNextLevel = experienceNeededPerLevel;

            BodyAspectLevel = MindAspectLevel = SoulAspectLevel = 0;
            bodyAspectPoints = mindAspectPoints = soulAspectPoints = aspectPointsPerLevelPerAspect;
            attributePoints = 0;
        }

        // initial calculation
        RecalculateAllStats();
    }

    // -------------------------
    // Public API used by other systems
    // -------------------------
    public int GetAttribute(AttributeType attr)
    {
        return baseAttributes[attr] + equipmentAttributeBonuses[attr];
    }

    public int GetRemainingAspectPoints(AspectType asp)
    {
        return asp switch
        {
            AspectType.Body => bodyAspectPoints,
            AspectType.Mind => mindAspectPoints,
            AspectType.Soul => soulAspectPoints,
            _ => 0
        };
    }

    public int GetRemainingAttributePoints() => attributePoints;

    // Spend an AspectPoint to raise an Aspect's level.
    // Each spent AspectPoint grants attributePointsPerAspectPoint attribute points into the global pool.
    public void SpendAspectPoint(AspectType asp)
    {
        switch (asp)
        {
            case AspectType.Body:
                if (bodyAspectPoints <= 0) { Debug.LogWarning("No Body aspect points."); return; }
                bodyAspectPoints--;
                BodyAspectLevel++;
                attributePoints += attributePointsPerAspectPoint;
                break;

            case AspectType.Mind:
                if (mindAspectPoints <= 0) { Debug.LogWarning("No Mind aspect points."); return; }
                mindAspectPoints--;
                MindAspectLevel++;
                attributePoints += attributePointsPerAspectPoint;
                break;

            case AspectType.Soul:
                if (soulAspectPoints <= 0) { Debug.LogWarning("No Soul aspect points."); return; }
                soulAspectPoints--;
                SoulAspectLevel++;
                attributePoints += attributePointsPerAspectPoint;
                break;
        }

        // recalc after spending
        RecalculateAllStats();
    }

    // Spend a global attribute point to increase an Attribute
    public void SpendAttributePoint(AttributeType attr)
    {
        if (attributePoints <= 0)
        {
            Debug.LogWarning("No attribute points.");
            return;
        }

        attributePoints--;
        baseAttributes[attr]++;
        RecalculateAllStats();
    }

    // Called by InventoryManager when equipping items (or on load)
    public void EquipFromInventory(Item hat, Item shirt, Item shoes)
    {
        equippedHat = hat;
        equippedShirt = shirt;
        equippedShoes = shoes;

        UpdateEquipmentBonuses();
        RecalculateAllStats();
    }

    // Public heal method used by InventoryManager
    public void Heal(int amount)
    {
        // CurrentHealth derived from MaxHealth; clamp handled in combat
        // Ensure CurrentHealth exists in persisted state: track current health as temp var locally
        // We'll manage a private currentHealth field persisted via save
        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        // notify any listeners
        TryInvokeStatsChanged();
    }

    // Public change gold
    public void ChangeGold(int amount)
    {
        CurrentGold += amount;
        TryInvokeStatsChanged();
    }

    // Public damage
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        TryInvokeStatsChanged();
    }

    // Experience gain (no intelligence scaling for now)
    public void GainExperience(float experience)
    {
        CurrentExperience += experience;
        while (CurrentExperience >= XPToNextLevel)
        {
            CurrentExperience -= XPToNextLevel;
            CurrentLevel++;

            // old behavior: grant aspectPointsPerLevelPerAspect to each aspect on level up
            bodyAspectPoints += aspectPointsPerLevelPerAspect;
            mindAspectPoints += aspectPointsPerLevelPerAspect;
            soulAspectPoints += aspectPointsPerLevelPerAspect;

            // notify level change if event exists
            // Example: GameEventsManager.instance.playerEvents.PlayerLevelChangeTo(CurrentLevel);
        }

        TryInvokeStatsChanged();
    }

    // -------------------------
    // Core recalculation
    // -------------------------
    // Public so UI/Inventory can call when menus close or equipment changed
    public void RecalculateAllStats()
    {
        // Combine base + equipment into effective attributes
        var effective = new Dictionary<AttributeType, int>();
        foreach (var kv in baseAttributes)
            effective[kv.Key] = kv.Value + equipmentAttributeBonuses[kv.Key];

        // Tier 3 calculation methods
        // base 10 HP per Vitality
        MaxHealth = effective[AttributeType.Vitality] * 10f;

        // Movement and combat basics
        MoveSpeed = 2f + effective[AttributeType.Agility] * 0.12f;
        MeleeDamage = effective[AttributeType.Strength] * 2f;
        MeleeSpeed = 1f + effective[AttributeType.Strength] * 0.05f;
        ThrowRange = 5f + effective[AttributeType.Alchemy] * 0.5f;
        ThrowSpeed = 5f + effective[AttributeType.Alchemy] * 0.2f;
        CastingRate = 1f + effective[AttributeType.Magic] * 0.1f;
        CriticalHitChance = effective[AttributeType.Luck] * 0.5f;
        DodgeChance = effective[AttributeType.Agility] * 0.4f;
        PotionEffectiveness = 1f + effective[AttributeType.Alchemy] * 0.1f;

        // Armor rating and gold find
        ArmorRating = (equippedHat?.GetArmorValue() ?? 0) + (equippedShirt?.GetArmorValue() ?? 0) + (equippedShoes?.GetArmorValue() ?? 0);
        GoldFindBonus = 1f + effective[AttributeType.Luck] * 0.01f; // 1% per Luck for extra gold

        // Ensure current health does not exceed max
        currentHealth = Mathf.Min(currentHealth, MaxHealth);

        // Fire event to notify UI and other systems
        TryInvokeStatsChanged();
    }

    // -------------------------
    // Equipment helpers
    // -------------------------
    private void UpdateEquipmentBonuses()
    {
        // Reset equipment bonuses
        foreach (AttributeType a in Enum.GetValues(typeof(AttributeType)))
            equipmentAttributeBonuses[a] = 0;

        // Add hat bonuses
        if (equippedHat != null)
            AddItemBonusesToEquipment(equippedHat);

        if (equippedShirt != null)
            AddItemBonusesToEquipment(equippedShirt);

        if (equippedShoes != null)
            AddItemBonusesToEquipment(equippedShoes);
    }

    private void AddItemBonusesToEquipment(Item item)
    {
        if (item == null) return;
        var dict = item.GetAttributeBonuses();
        foreach (var kv in dict)
        {
            equipmentAttributeBonuses[kv.Key] += kv.Value;
        }
    }

    // -------------------------
    // Save / Load
    // -------------------------
    // Persist minimal state
    private bool LoadStats()
    {
        try
        {
            if (!File.Exists(savePath)) return false;
            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return false;

            CurrentLevel = data.CurrentLevel;
            CurrentExperience = data.CurrentExperience;
            CurrentGold = data.CurrentGold;
            CurrentArmor = data.CurrentArmor;

            BodyAspectLevel = data.BodyAspectLevel;
            MindAspectLevel = data.MindAspectLevel;
            SoulAspectLevel = data.SoulAspectLevel;

            bodyAspectPoints = data.BodyAspectPoints;
            mindAspectPoints = data.MindAspectPoints;
            soulAspectPoints = data.SoulAspectPoints;

            attributePoints = data.AttributePoints;

            baseAttributes.Clear();
            foreach (var entry in data.Attributes)
            {
                if (Enum.TryParse(entry.AttributeName, out AttributeType at))
                    baseAttributes[at] = entry.Value;
            }

            // Reconstruct items from InventoryManager's database (InventoryManager must expose GetItemByID)
            equippedHat = InventoryManager.Instance != null && !string.IsNullOrEmpty(data.EquippedHatID) ? InventoryManager.Instance.GetItemByID(data.EquippedHatID) : null;
            equippedShirt = InventoryManager.Instance != null && !string.IsNullOrEmpty(data.EquippedShirtID) ? InventoryManager.Instance.GetItemByID(data.EquippedShirtID) : null;
            equippedShoes = InventoryManager.Instance != null && !string.IsNullOrEmpty(data.EquippedShoesID) ? InventoryManager.Instance.GetItemByID(data.EquippedShoesID) : null;

            // set XPToNextLevel
            XPToNextLevel = experienceNeededPerLevel;

            // currentHealth is derived but try to clamp to Max after recalc
            currentHealth = MaxHealth;

            UpdateEquipmentBonuses();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load player stats: " + e);
            return false;
        }
    }

    public void SaveStats()
    {
        try
        {
            var data = new SaveData
            {
                CurrentLevel = CurrentLevel,
                CurrentExperience = CurrentExperience,
                CurrentGold = CurrentGold,
                CurrentArmor = CurrentArmor,
                BodyAspectLevel = BodyAspectLevel,
                MindAspectLevel = MindAspectLevel,
                SoulAspectLevel = SoulAspectLevel,
                BodyAspectPoints = bodyAspectPoints,
                MindAspectPoints = mindAspectPoints,
                SoulAspectPoints = soulAspectPoints,
                AttributePoints = attributePoints,
                Attributes = new List<AttributeEntry>(),
                EquippedHatID = equippedHat != null ? equippedHat.ID : "",
                EquippedShirtID = equippedShirt != null ? equippedShirt.ID : "",
                EquippedShoesID = equippedShoes != null ? equippedShoes.ID : ""
            };

            foreach (var kv in baseAttributes)
                data.Attributes.Add(new AttributeEntry { AttributeName = kv.Key.ToString(), Value = kv.Value });

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save player stats: " + e);
        }
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }

    // -------------------------
    // Internal utilities & state
    // -------------------------
    private float currentHealth = 1f;

    // Fire whatever stat change mechanism your project has.
    // We use a safe call to a delegate-like field if present.
    private void TryInvokeStatsChanged()
    {
        try
        {
            // If your GameEventsManager has a delegate or method named StatsChanged, this will attempt to invoke it safely.
            // If your project exposes StatsChanged as an Action or delegate field, this will work:
            GameEventsManager.instance.playerEvents.StatsChanged();
        }
        catch
        {
            // If the project uses a different notification pattern, ignore here.
        }
    }
}
