using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{


    public Image image;
    //public Image TopImage;
    public Color selectedColor, notSelectedColor;
    public string name;
    [SerializeField] private GameObject selectedItemShower; //vie inspectorista



    private void Awake()
    {
        Deselect();

        selectedItemShower.GetComponent<Image>().enabled = false;
    }

    public void Select()
    {
        image.color = selectedColor;
        selectedItemShower.GetComponent<Image>().enabled = true;

    }
    public void Deselect()
    {
        image.color = notSelectedColor;
        selectedItemShower.GetComponent<Image>().enabled = false;

    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        InventoryItem inventoryItem = dropped.GetComponent<InventoryItem>();
        inventoryItem.parentAfterDrag = transform;

    }
    
}
