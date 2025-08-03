using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDialogueAction : MonoBehaviour
{
    [SerializeField] Item Sword;
    [SerializeField] Item Potion;

    GameObject player;

    public ShopManager shopManager;

    [SerializeField] Item questItem;
    public int count;

    private void Start()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
        if (shopManager == null)
        {
            Debug.LogError("ShopManager null");
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void QuestReward()
    {
        InventoryManager.Instance.AddItem(Sword);
    }

    public void HasSpokentoTheLeader()
    {
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
        OpenQuest(0, null);
    }

    public void OpenSlauhterQuest()
    {
        OpenQuest(1, null);
    }

    public void OpenChocolateQuest()
    {
        OpenQuest(2, questItem);
    }

    public void OpenStoneQuest()
    {
        OpenQuest(3, questItem);
    }

    private void OpenQuest(int quest, Item item)
    {
        Quest.QuestState state = QuestManager.instance.allQuests[quest].state;
        if (state == Quest.QuestState.CAN_START)
        {
            QuestManager.instance.StartQuest(QuestManager.instance.allQuests[quest]);
        }
        else if (state == Quest.QuestState.CAN_FINISH)
        {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[quest]);
        }
        else if (InventoryManager.Instance.HasOrConsumeItemAmount(item, count, true))
        {
            QuestManager.instance.CompleteQuest(QuestManager.instance.allQuests[quest]);
        }
    }
}