using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Item[] startItems;

    public InventorySlot[] inventorySlots;
    public GameObject InventoryItemPrefab;

    int selectedSlot = 0; // Ensimm‰inen slotti oletuksena valittuna.

    void ChangeSelectedSlot(int NewValue)
    {
        if (NewValue < 0 || NewValue >= inventorySlots.Length)
        {
            Debug.LogWarning("Tried to select a slot outside valid range.");
            return;
        }

        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[NewValue].Select();
        selectedSlot = NewValue;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple InventoryManager instances detected. Destroying duplicate.");
        }
    }


    private void Start()
    {
        ChangeSelectedSlot(0);
        foreach (var item in startItems)
        {
            AddItem(item);
            
        }

        Debug.Log(selectedSlot);
    }


    private void Update()
    {
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number <= inventorySlots.Length) 
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < 10 &&
                itemInSlot.item.stackable == true)
            {
                Debug.Log($"Found stackable item in slot {i}");

                itemInSlot.count++;
                itemInSlot.RefrestCount();
                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot) //Ei tarvitse koskea 
    {
        GameObject newItemGo = Instantiate(InventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.IntialiseItem(item);
    }

    public Item GetSelectedItem(bool use)
    {
        if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length)
        {
            Debug.LogWarning("Selected slot is out of range.");
            return null;
        }

        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if (use == true)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefrestCount();
                }
            }

            return item;
        }

        return null;
    }

    public bool FindAndConsumeItem(Item itemToFind, bool consume)
    {
        // T‰m‰ funktio voisi nyt kutsua uutta, monipuolisempaa funktiota:
        return HasOrConsumeItemAmount(itemToFind, 1, consume);
    }

    // ----- UUSI FUNKTIO TƒSSƒ -----
    /// <summary>
    /// Etsii annettua esinetyyppi‰ ja -m‰‰r‰‰ koko inventaariosta.
    /// Jos lˆytyy riitt‰v‰sti, voi halutessaan kuluttaa ne.
    /// Kulutus tapahtuu tarvittaessa useammasta pinosta.
    /// </summary>
    /// <param name="itemToFind">Etsitt‰v‰ Item ScriptableObject.</param>
    /// <param name="amountToHandle">Etsitt‰v‰/kulutettava m‰‰r‰. T‰m‰n tulee olla positiivinen luku.</param>
    /// <param name="consume">Jos true, v‰hent‰‰ esineiden m‰‰r‰‰ ja poistaa ne tarvittaessa.</param>
    /// <returns>True, jos esineit‰ lˆytyi riitt‰v‰sti (ja mahdollisesti kulutettiin, jos consume=true), muuten false.</returns>
    public bool HasOrConsumeItemAmount(Item itemToFind, int amountToHandle, bool consume)
    {
        if (itemToFind == null)
        {
            Debug.LogWarning("ItemToFind cannot be null in HasOrConsumeItemAmount.");
            return false;
        }
        if (amountToHandle <= 0)
        {
            Debug.LogWarning($"AmountToHandle ({amountToHandle}) must be greater than 0 in HasOrConsumeItemAmount.");
            return false; // Ei voida k‰sitell‰ nollaa tai negatiivista m‰‰r‰‰ t‰ll‰ logiikalla
        }

        List<InventoryItem> itemInstancesInInventory = new List<InventoryItem>();
        int totalAvailableCount = 0;

        // Vaihe 1: Etsi kaikki esineen instanssit ja laske kokonaism‰‰r‰
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            // Huom: GetComponentInChildren etsii myˆs itse objektista, jos komponentti on siin‰.
            // Jos InventoryItem on AINA lapsiobjekti, t‰m‰ on ok.
            // Jos se voisi olla myˆs samassa objektissa slotin kanssa, t‰m‰ on myˆs ok.
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null && itemInSlot.item == itemToFind)
            {
                itemInstancesInInventory.Add(itemInSlot);
                totalAvailableCount += itemInSlot.count;
            }
        }

        // Vaihe 2: Tarkista onko tarpeeksi esineit‰
        if (totalAvailableCount < amountToHandle)
        {
            return false; // Ei tarpeeksi esineit‰ inventaariossa
        }

        // Vaihe 3: Jos vain tarkistetaan (ei kuluteta) ja esineit‰ on tarpeeksi, palauta true
        if (!consume)
        {
            return true;
        }

        // Vaihe 4: Kuluta esineet (jos consume == true ja niit‰ on tarpeeksi)
        int amountLeftToConsume = amountToHandle;
        foreach (InventoryItem itemInstance in itemInstancesInInventory)
        {
            if (amountLeftToConsume <= 0)
            {
                break; // Tarvittava m‰‰r‰ on jo kulutettu
            }

            int amountToTakeFromThisStack = Mathf.Min(itemInstance.count, amountLeftToConsume);

            itemInstance.count -= amountToTakeFromThisStack;
            amountLeftToConsume -= amountToTakeFromThisStack;

            if (itemInstance.count <= 0)
            {
                Destroy(itemInstance.gameObject); // Tuhoa esineen GameObject, jos m‰‰r‰ menee nollaan
            }
            else
            {
                itemInstance.RefrestCount(); // P‰ivit‰ esineen n‰ytt‰m‰ m‰‰r‰ (huom: mahdollinen kirjoitusvirhe "RefreshCount")
            }
        }

        // Varmistus (t‰m‰n ei pit‰isi tapahtua, jos logiikka on oikein)
        if (amountLeftToConsume > 0)
        {
            Debug.LogError($"InventoryManager: Virhe esineiden kulutuksessa. Yritettiin kuluttaa {amountToHandle} kpl esinett‰ {itemToFind.name}, mutta {amountLeftToConsume} j‰i kuluttamatta. T‰m‰ viittaa logiikkavirheeseen.");
            // T‰ss‰ voisi teoriassa yritt‰‰ peruuttaa tehdyt muutokset, mutta se monimutkaistaisi huomattavasti.
            // T‰ss‰ vaiheessa palautetaan false, koska kaikkea pyydetty‰ ei saatu kulutettua.
            return false;
        }

        return true; // Esineet lˆytyiv‰t ja kulutus onnistui (jos consume oli true)
    }
    // ----- UUDEN FUNKTION LOPPU -----
}

