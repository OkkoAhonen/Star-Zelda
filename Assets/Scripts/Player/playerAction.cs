using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerAction : MonoBehaviour
{
    public PlayerStats playerStats;

    [SerializeField] private DialogueUI dialogueUI; // Dialogia kontrolloidaan tästä skriptistä
    public DialogueUI DialogueUI => dialogueUI; // Korjattu: palauttaa yksityisen muuttujan
    public Interactable Interactable { get; set; }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        playerStats.playerMaxHealth = 100;
        playerStats.playerCurrentHealth = playerStats.playerMaxHealth;
    }

    public void playerTakeDamage(float damage)
    {
        playerStats.playerCurrentHealth -= damage;
        Debug.Log(playerStats.playerCurrentHealth);
    }


    private void Update()
    {
        //if (/*DialogueUI.IsOpen &&*/ SceneManager.GetActiveScene().buildIndex != 1) return;


        if (Input.GetKeyDown(KeyCode.E))
        {
            Interactable?.Interact(this);
        }

        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        if(equippedItem!= null) { 
        if(equippedItem.type == ItemType.Food)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                playerStats.playerCurrentHealth += equippedItem.foodHeal;
                InventoryManager.Instance.GetSelectedItem(true);
                Debug.Log("Food healed "+ equippedItem.foodHeal+ "Health back");
            }
        }
    }
    }

}