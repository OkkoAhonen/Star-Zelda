using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemImageScript : MonoBehaviour
{
    [HideInInspector] public Image image;          // Viittaus Image-komponenttiin
    public Sprite itemSprite;    // Sprite, joka halutaan n‰ytt‰‰
    private void Awake()
    {
        // Haetaan Image-komponentti, jos sit‰ ei ole asetettu Inspectorissa
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        // Asetetaan sprite, jos se on m‰‰ritelty
        if (itemSprite != null && image != null)
        {
            image.sprite = itemSprite;
        }
        else
        {
            Debug.LogWarning("ShopItemImageScript: itemSprite tai image on null");
        }
    }


    public void setSprite(Sprite newsprite)
    {
        if(image != null)
        {
            image.sprite = newsprite;
        }
    }
}