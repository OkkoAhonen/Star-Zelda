using System.Collections.Generic;
using UnityEngine;

// Base item data. Use derived SOs to categorize items in the inspector.
[CreateAssetMenu(menuName = "Game Data/Item/Base")]
public class Item : ScriptableObject
{
    // Stable unique ID (for saves). Keep this consistent for an asset.
    public string ID;
    public string DisplayName;
    public Sprite Image;
    [TextArea] public string Description;

    // Stacking
    public bool Stackable = true;
    public int MaxStack = 99;

    // Simple consumable heal amount
    public int FoodHeal = 0;

    // Armor
    public bool IsArmor = false;
    // 0 = hat, 1 = shirt, 2 = shoes
    public int ArmorSlotIndex = -1;
    public int ArmorValue = 0;

    // General combat fields (optional)
    public bool IsWeapon = false;
    public float MeleeDamage = 0f;
    public float MeleeSpeed = 0f;
    public bool IsBow = false;
    public bool IsPotion = false;
    public float PotionEffectValue = 0f;

    // Attribute bonuses (flat)
    [SerializeField] private AttributeBonus[] attributeBonuses = new AttributeBonus[0];

    public Dictionary<PlayerStatsManager.AttributeType, int> GetAttributeBonuses()
    {
        var dict = new Dictionary<PlayerStatsManager.AttributeType, int>();
        for (int i = 0; i < attributeBonuses.Length; i++)
        {
            var b = attributeBonuses[i];
            if (!dict.ContainsKey(b.Attribute))
                dict[b.Attribute] = b.Value;
            else
                dict[b.Attribute] += b.Value;
        }
        return dict;
    }

    public int GetArmorValue()
    {
        return IsArmor ? ArmorValue : 0;
    }

    [System.Serializable]
    public struct AttributeBonus
    {
        public PlayerStatsManager.AttributeType Attribute;
        public int Value;
    }
}
