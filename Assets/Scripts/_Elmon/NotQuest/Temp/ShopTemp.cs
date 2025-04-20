using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class ShopTemp : MonoBehaviour
{
    public GameObject shopMenu;
    public List<Perk> availablePerks; // Populate these in the inspector.
    public List<Perk> unlockedPerks = new List<Perk>();
    public bool canShop;
    public Transform itemsList;
    public GameObject shopItemPrefab;

    private void Start()
    {
        shopMenu.SetActive(false);
        // Spawn shop item UI elements for each available perk.
        foreach (var perk in availablePerks)
        {
            GameObject item = Instantiate(shopItemPrefab, itemsList);
            ShopItemUI ui = item.GetComponent<ShopItemUI>();
            if (ui != null)
            {
                ui.Initialize(perk);
            }
        }
        // Order the items initially.
        ReorderItems();
    }

    private void ToggleShop(InputEventContext context)
    {
        if (canShop)
        {
            shopMenu.SetActive(!shopMenu.activeSelf);
        }
    }

    // Reorders shop item UI elements in itemsList.
    public void ReorderItems()
    {
        // Get all ShopItemUI components under itemsList.
        ShopItemUI[] items = itemsList.GetComponentsInChildren<ShopItemUI>();
        // Split into not-bought and bought lists.
        List<ShopItemUI> notBought = new List<ShopItemUI>();
        List<ShopItemUI> bought = new List<ShopItemUI>();

        foreach (var item in items)
        {
            if (unlockedPerks.Contains(item.perkData))
                bought.Add(item);
            else
                notBought.Add(item);
        }
        // Sort both lists by cost ascending.
        notBought.Sort((a, b) => a.Cost.CompareTo(b.Cost));
        bought.Sort((a, b) => a.Cost.CompareTo(b.Cost));
        // Combine: notBought first, then bought.
        List<ShopItemUI> combined = new List<ShopItemUI>(notBought);
        combined.AddRange(bought);
        // Set sibling indices according to combined order.
        for (int i = 0; i < combined.Count; i++)
        {
            combined[i].transform.SetSiblingIndex(i);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canShop)
            if (collision.transform.name == "Player")
                canShop = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (canShop)
            if (collision.transform.name == "Player")
                canShop = false;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.OnSubmitPressed += ToggleShop;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.OnSubmitPressed -= ToggleShop;
    }
}
