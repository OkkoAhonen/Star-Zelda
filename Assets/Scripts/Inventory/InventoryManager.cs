using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Item[] startItems;

    public PlayerMovement2D playerMovement;

    public Item[] ShopItem1;

    public InventorySlot[] inventorySlots;
    public GameObject InventoryItemPrefab;

    int selectedSlot = 0; // Ensimmäinen slotti oletuksena valittuna.

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
        Instance = this;
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

    public void OstaShopItem1()
    {
        playerMovement.money -= 90;
        foreach (var item in ShopItem1)
        {
            AddItem(item);
            

        }
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
                itemInSlot.count < 4 &&
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
}
