using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueAction : MonoBehaviour
{
    [SerializeField] Item Sword;
    [SerializeField] Item Potion;

    public GameObject player;

    public ShopManager shopManager;


    public Quest[] quests;

    private void Awake()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
    }

    private void Start()
    {
        if(shopManager == null)
        {
            Debug.LogError("ShopManager null");
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void addSwordToInventory()
    {
        InventoryManager.Instance.AddItem(Sword);
        TownManager.influence += 1;
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

    public void AvaaSepanKauppa()
    {
        shopManager.OpenSepanKauppa();
    }
    public void AvaaNormiKauppa()
    {
        shopManager.OpenShop();
        AudioManager.instance.PlaySFX("Inventory");

        if (quests[0].state == Quest.QuestState.CAN_START) { 
        QuestManager.instance.StartQuest(quests[0]);
        }
        else if ((quests[0].state == Quest.QuestState.CAN_FINISH)){
            QuestManager.instance.CompleteQuest(quests[0]);
        }
    }

    public void avaaQuest()
    {
        
        QuestManager.instance.StartQuest(quests[0]);
    }

}
