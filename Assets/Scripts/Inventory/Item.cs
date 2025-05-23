using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Game Data/Item")]


public class Item : ScriptableObject
{

    [Header("Only gameplay")]
    public TileBase tile;
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);
    public float price = 25f;

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;

    [Header("Melee Combat Properties")]
    public float attackDamage = 0f; //katso combat script
    public float damageBooster = 1f; //jkatso combat script
    public float attackRadius = 1.5f;
    public float maxChargeTime = 3f;
    public bool isWeapon = false; // Voit m��ritt��, onko kyseinen item ase.

    [Header("Potion Combat Properties")]
    public float potionAttackDamage = 20f;
    public float potionActivetimer = 2f;
    public float damageDuration = 0.5f;
    public bool isDamagePotion;
    public bool isHealthPotion;
    public float PotionHeal = 10f;

    [Header("archer Combat Properties")]
    public float accurasu = 20f;
    public bool isBow = false;

    [Header("Food")]
    public float foodHeal = 20f;


    [Header("QuestItem")]
    public bool isItVillapaita = false;

}

public enum ItemType
{
    BuildingBlock,
    Tool,
    Weapon,
    potion,
    Bow,
    Food,
    Mushroom,
    GemStone,
    Blood,
    Stone,
    QuestItem

}

public enum ActionType
{
    Dig,
    Mine,
    Slash,
    potionthrow
    
}

