using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public Item[] shopItems;                    // Lista kaupan tavaroista
    public GameObject itemsToBuyPrefab;         // Viittaus ItemsToBuy-prefabiin (asetetaan Inspectorissa)
    public Transform shopContent;               // Viittaus Shop1:n Transformiin (asetetaan Inspectorissa)

    [HideInInspector] public GameObject inventoryManagerGameObject;
    public MoneyManager moneyManager;
    public InventoryManager inventoryManager;

    public GameObject shop;                     // Kaupan UI-paneeli (asetetaan Inspectorissa)
    public GameObject ExitButton;               // Poistu-painike (asetetaan Inspectorissa)

    void Start()
    {
        inventoryManagerGameObject = GameObject.FindGameObjectWithTag("InventoryManager");
        moneyManager = inventoryManagerGameObject.GetComponent<MoneyManager>();
        inventoryManager = inventoryManagerGameObject.GetComponent<InventoryManager>();

        Debug.Log("ShopManager alustettu");
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        ExitButton.SetActive(true);
        InitializeShopItems(); // Alustaa kaupan tavarat
    }

    public void CloseShop()
    {
        shop.SetActive(false);
        ExitButton.SetActive(false);
    }

    public void InitializeShopItems()
    {
        // Poista vanhat ItemsToBuy-objetit Shop1:st‰
        foreach (Transform child in shopContent)
        {
            Destroy(child.gameObject);
        }

        // Luo uudet ItemsToBuy-objetit shopItems-listan perusteella
        for (int i = 0; i < shopItems.Length; i++)
        {
            // Luo uusi instanssi prefabista Shop1:n lapseksi
            GameObject newItem = Instantiate(itemsToBuyPrefab, shopContent);

            // Aseta tavaran tiedot ShopItemDisplay-skriptiin
            ShopItemDisplay display = newItem.GetComponent<ShopItemDisplay>();
            if (display != null)
            {
                display.SetItem(shopItems[i]);
            }
            else
            {
                Debug.LogWarning("ShopItemDisplay-komponentti puuttuu prefabista!");
            }

            // Lis‰‰ napin toiminnallisuus
            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                int index = i; // Tarvitaan lambda-funktiota varten
                button.onClick.AddListener(() => BuyItem(index));
            }
        }
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