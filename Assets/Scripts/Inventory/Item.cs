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

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;

    [Header("Melee Combat Properties")]
    public float attackDamage = 0f; //katso combat script
    public float damageBooster = 1f; //jkatso combat script
    public float attackRadius = 1.5f;
    public float maxChargeTime = 3f;
    public bool isWeapon = false; // Voit m‰‰ritt‰‰, onko kyseinen item ase.

    [Header("Potion Combat Properties")]
    public float potionAttackDamage = 20f;
    public float potionActivetimer = 2f;
    public float damageDuration = 0.5f;
    public bool isPotion = false;

    [Header("archer Combat Properties")]
    public float accurasu = 20f;
    public bool isBow = false;

}

public enum ItemType
{
    BuildingBlock,
    Tool,
    Weapon,
    potion
}

public enum ActionType
{
    Dig,
    Mine,
    Slash,
    potionthrow
    
}

