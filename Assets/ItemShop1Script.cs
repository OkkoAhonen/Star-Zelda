using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShop1Script : MonoBehaviour
{
    public int itemsToBuy = 3;
    public Item item;

    public ShopItemImageScript shopItemImageScript;
    public ItemTextScript itemtextScript;
    public ShopItemPriceScript shopItemPriceScript;


   
    // Start is called before the first frame update

    private void Awake()
    {
        shopItemPriceScript = GetComponentInChildren<ShopItemPriceScript>();
        itemtextScript = GetComponentInChildren<ItemTextScript>();
        shopItemImageScript = GetComponentInChildren<ShopItemImageScript>();

    }




    public void InitializeShopItems(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("InitializeShopItems: item on null!");
            return;
        }

        shopItemImageScript.setSprite(item.image);
        shopItemPriceScript.SetPrice((int)item.price);
        itemtextScript.SetText(item.name);
    }


}
