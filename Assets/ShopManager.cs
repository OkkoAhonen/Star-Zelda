using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public Item[] shopItems;
    public int shopCount = 1;

    [HideInInspector] public GameObject inventoryManagerGameObject;
    public MoneyManager moneyManager;
    public InventoryManager inventoryManager;
    public ItemShop1Script itemShop1Script;

    public GameObject shop; //Aseta Inspectorissa
    public GameObject ExitButton;//Aseta Inspectorissa

    public ShopItemDisplay[] shopDisplays;

    void Start()
    {
        inventoryManagerGameObject = GameObject.FindGameObjectWithTag("InventoryManager");
        moneyManager = inventoryManagerGameObject.GetComponent<MoneyManager>();
        inventoryManager = inventoryManagerGameObject.GetComponent<InventoryManager>();

        shopCount = shopItems.Length;
        Debug.Log("ShopManager alustettu");
    }

    // Uusi metodi itemien alustamiseen
    public void InitializeShopItems()
    {
        for (int i = 0; i < shopItems.Length && i < shopDisplays.Length; i++)
        {
            if (shopDisplays[i] != null)
            {
                shopDisplays[i].SetItem(shopItems[i]);
            }
            else
            {
                Debug.LogWarning($"shopDisplays[{i}] on null!");
            }
        }
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        ExitButton.SetActive(true);
        InitializeShopItems(); // Alustaa itemit, kun kauppa avataan
    }

    public void CloseShop()
    {
        shop.SetActive(false); // Korjattu: sulkee kaupan
        ExitButton.SetActive(false);
    }

    public void BuyItem(int index)
    {
        if (index >= 0 && index < shopItems.Length)
        {
            Item item = shopItems[index];
            moneyManager.ChangeMoney(-item.price);
            inventoryManager.AddItem(item);
        }
    }
}