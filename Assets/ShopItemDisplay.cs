using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemDisplay : MonoBehaviour
{
    private ShopItemImageScript imageScript;
    private ItemTextScript textScript;
    private ShopItemPriceScript priceScript;

    private void Awake()
    {
        // Hakee napin lapsi-objektien skriptit
        imageScript = GetComponentInChildren<ShopItemImageScript>();
        textScript = GetComponentInChildren<ItemTextScript>();
        priceScript = GetComponentInChildren<ShopItemPriceScript>();
    }

    public void SetItem(Item item)
    {
        if (item != null && imageScript != null && textScript != null && priceScript != null)
        {
            imageScript.setSprite(item.image);
            textScript.SetText(item.name);
            priceScript.SetPrice((int)item.price);
            Debug.Log("Laitettu");
        }
        else
        {
            Debug.Log("jotain meni pieleen ShopItemDispaly");
        }
    }
}
