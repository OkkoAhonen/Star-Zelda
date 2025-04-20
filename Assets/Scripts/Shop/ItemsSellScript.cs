using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsSellScript : MonoBehaviour
{

    public MoneyManager moneyManager;
    // Start is called before the first frame update
    private void Awake()
    {
        moneyManager = GameObject.Find("InventoryManager").GetComponent<MoneyManager>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void sellItem()
    {
        GameEventsManager.instance.playerEvents.GainExperience(25);
        Item soldItem = InventoryManager.Instance.GetSelectedItem(true);
        if (soldItem != null) { 
        moneyManager.ChangeMoney(soldItem.price);
        }
        else { Debug.Log("Ei itemiä kädessä"); }

    }
}
