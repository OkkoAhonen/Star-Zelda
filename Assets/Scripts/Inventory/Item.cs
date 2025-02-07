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

    [Header("Combat Properties")]
    public int attackDamage = 0;
    public float attackRange = 1.5f; // Esim. kuinka kaukana pelaaja voi osua miekalla.
    public bool isWeapon = false; // Voit m‰‰ritt‰‰, onko kyseinen item ase.


}

public enum ItemType
{
    BuildingBlock,
    Tool,
    Weapon
}

public enum ActionType
{
    Dig,
    Mine,
    Slash
}

