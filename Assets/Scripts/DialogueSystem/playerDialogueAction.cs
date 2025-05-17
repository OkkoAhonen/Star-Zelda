using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDialogueAction : MonoBehaviour
{
    [SerializeField] Item Sword;
    [SerializeField] Item Potion;

    public GameObject player;

    public ShopManager shopManager;

    public DialogueObject GolemTankyou;
    public Quest[] quests;

    public Item ChocolateItem;
    public Item StoneItem;

    private void Awake()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
    }

    private void Start()
    {
        if (shopManager == null)
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
        {
            shopManager.OpenShop();
            AudioManager.instance.PlaySFX("Inventory");

        }
    }

    public void OpenVillageQuest1()
    {
        if (QuestManager.instance.allQuests[0].state == Quest.QuestState.CAN_START)
        {
            QuestManager.instance.StartQuest(QuestManager.instance.allQuests[0]);
        }
        else if (QuestManager.instance.allQuests[0].state == Quest.QuestState.CAN_FINISH)
        {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[0]);
        }
    }

    public void OpenSlauhterQuest()
    {
        

        if (QuestManager.instance.allQuests[1].state == Quest.QuestState.CAN_START)
        {
            QuestManager.instance.StartQuest(QuestManager.instance.allQuests[1]);
        }
        else if (QuestManager.instance.allQuests[1].state == Quest.QuestState.CAN_FINISH)
        {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[1]);
        }

    }

    public void OpenChocolateQuest()
    {
        if (QuestManager.instance.allQuests[2].state == Quest.QuestState.CAN_START)
        {
            QuestManager.instance.StartQuest(QuestManager.instance.allQuests[2]);
        }
        else if (InventoryManager.Instance.FindAndConsumeItem(ChocolateItem, true)) {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[2]);
        }
    }

    public void OpenStoneQuest()
    {
        if (QuestManager.instance.allQuests[3].state == Quest.QuestState.CAN_START)
        {
            QuestManager.instance.StartQuest(QuestManager.instance.allQuests[3]);
        }
       else if (InventoryManager.Instance.HasOrConsumeItemAmount(StoneItem, 10, true))
        {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[3]);
        }
    }
}