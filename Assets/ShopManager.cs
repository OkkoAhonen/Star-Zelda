using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public Item[] shopItems;                    // Lista kaupan tavaroista
    public Item[] sepanItems;                   // Lista Sep‰n Itemeille

    public GameObject itemsToBuyPrefab;         // Viittaus ItemsToBuy-prefabiin (asetetaan Inspectorissa)
    public Transform shopContent;               // Viittaus Shop1:n Transformiin (asetetaan Inspectorissa)

    public GameObject sepanKauppa;              // Viittaa sep‰n kauppaobjektiin
    public Transform sepanContent;              // Viittaa sep‰n kaupan sis‰ltˆobjektiin (esim. SepanShop1)

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

        // Varmistetaan, ett‰ molemmat kaupat ovat aluksi pois p‰‰lt‰
        shop.SetActive(false);
        sepanKauppa.SetActive(false);
        ExitButton.SetActive(false);

        Debug.Log("ShopManager alustettu");
    }

    public void OpenShop()
    {
        // Varmista, ett‰ sep‰n kauppa on pois p‰‰lt‰
        sepanKauppa.SetActive(false);
        // Aktivoi peruskauppa
        shop.SetActive(true);
        ExitButton.SetActive(true);
        InitializeShopItems(shopItems, shopContent);
        Debug.Log("Peruskauppa avattu");
    }

    public void OpenSepanKauppa()
    {
        // Varmista, ett‰ peruskauppa on pois p‰‰lt‰
        shop.SetActive(false);
        // Aktivoi sep‰n kauppa
        sepanKauppa.SetActive(true);
        ExitButton.SetActive(true);
        InitializeShopItems(sepanItems, sepanContent);
        Debug.Log("Sep‰n kauppa avattu");
    }

    // Sulje peruskauppa
    public void CloseShop()
    {
        shop.SetActive(false);
        ExitButton.SetActive(false);
    }

    // Sulje sep‰n kauppa
    public void CloseSepanKauppa()
    {
        sepanKauppa.SetActive(false);
        ExitButton.SetActive(false);
    }

    private void InitializeShopItems(Item[] items, Transform content)
    {
        Debug.Log("Alustetaan kauppa: " + content.name + ", tavaroita: " + items.Length);
        // Poista vanhat ItemsToBuy-objetit
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Luo uudet ItemsToBuy-objetit
        for (int i = 0; i < items.Length; i++)
        {
            GameObject newItem = Instantiate(itemsToBuyPrefab, content);
            ShopItemDisplay display = newItem.GetComponent<ShopItemDisplay>();
            if (display != null)
            {
                display.SetItem(items[i]);
            }
            else
            {
                Debug.LogWarning("ShopItemDisplay-komponentti puuttuu prefabista!");
            }

            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => BuyItem(items, index));
            }
        }
    }

    private void BuyItem(Item[] items, int index)
    {
        if (index >= 0 && index < items.Length)
        {
            Item item = items[index];
            moneyManager.ChangeMoney(-item.price);
            inventoryManager.AddItem(item);
        }
    }
}