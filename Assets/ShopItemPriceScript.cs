using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopItemPriceScript : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI; // Viittaus TextMeshProUGUI-komponenttiin
    public int price = 50;                  // Hinta, asetetaan Inspectorissa
    public string text;                     // Lopullinen näytettävä teksti (ei oletustekstiä tässä)

    void Start()
    {
        // Haetaan TextMeshProUGUI-komponentti, jos ei asetettu Inspectorissa
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        // Asetetaan teksti, jos komponentti löytyy
        if (textMeshProUGUI != null)
        {
            UpdatePriceText(); // Päivitetään teksti funktiolla
        }
        else
        {
            Debug.LogWarning("ShopItemPriceScript: TextMeshProUGUI-komponentti puuttuu!");
        }
    }

    // Funktio hinnan päivittämiseen
    private void UpdatePriceText()
    {
        text = $"{price} $"; // Interpolointi: yhdistää hinnan ja valuuttamerkin
        textMeshProUGUI.text = text;
    }

    // Bonus: Mahdollisuus muuttaa hintaa pelin aikana
    public void SetPrice(int newPrice)
    {
        price = newPrice;
        if (textMeshProUGUI != null)
        {
            UpdatePriceText();
        }
    }
}