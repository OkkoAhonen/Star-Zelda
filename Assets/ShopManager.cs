using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public Item[] shopItems;
    public int shopCount = 1;
    public GameObject shopPrefab;

    [HideInInspector] public GameObject inventoryManagerGameObject;
    public MoneyManager moneyManager;
    public InventoryManager inventoryManager;
    public ItemShop1Script itemShop1Script;

    public ShopItemDisplay[] shopDisplays;
    public GameObject shopGameObject; //Aseta inspectorissa


    // Start is called before the first frame update
    void Start()
    {
        
        inventoryManagerGameObject = GameObject.FindGameObjectWithTag("InventoryManager");
        moneyManager = inventoryManagerGameObject.GetComponent<MoneyManager>();
        inventoryManager = inventoryManagerGameObject.GetComponent<InventoryManager>();

        shopCount = shopItems.Length;
        Debug.Log("Laitettu2");
        for (int i = 0; i < shopItems.Length && i < shopDisplays.Length; i++)
        {
            shopDisplays[i].SetItem(shopItems[i]);
        }
    }
    //BuyItem funktiot aktivoituvat kun painaa buttonia
    public void BuyItem(int index)
    {
        if (index >= 0 && index < shopItems.Length)
        {
            Item item = shopItems[index];
            moneyManager.ChangeMoney(-item.price);
            inventoryManager.AddItem(item);
        }
    }

    public void OpenShop()
    {
        GameObject shop = GameObject.FindWithTag("Shop");
        shopGameObject.SetActive(true);
    }
    public void CloseShop()
    {
        GameObject shop = GameObject.FindWithTag("Shop");
        shop.SetActive(true);
    }


}
