using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] ItemsToPickup;

    public void PickupItem(int id)
    {
        bool result = inventoryManager.AddItem(ItemsToPickup[id]);
        if(result == true)
        {
            Debug.Log("Item Added");
        }
        else
        {
            Debug.Log("Item not added");
        }
    }

    public void GetSelectedItem()
    {
        Item recievedItem = inventoryManager.GetSelectedItem(false);
        if(recievedItem != null)
        {
            Debug.Log("Recieved Item");
        }
        else
        {
            Debug.Log("Did not recieve Item");
        }
    }

    public void UseSelectedItem()
    {
        Item recievedItem = inventoryManager.GetSelectedItem(true);
        if (recievedItem != null)
        {
            Debug.Log("Recieved Item");
        }
        else
        {
            Debug.Log("Did not recieve Item");
        }
    }



}
