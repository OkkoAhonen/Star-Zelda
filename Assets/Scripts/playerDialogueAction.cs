using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueAction : MonoBehaviour
{
    [SerializeField] Item Sword;
    [SerializeField] Item Potion;

    public GameObject player;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void addSwordToInventory()
    {
        InventoryManager.Instance.AddItem(Sword);
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.PlusStartPoints();
    }

    public void addPotionToInventory()
    {
        InventoryManager.Instance.AddItem(Potion);
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.PlusStartPoints();
    }

    public void HasSpokentoTheLeader()
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.PlusStartPoints();
    }

}
