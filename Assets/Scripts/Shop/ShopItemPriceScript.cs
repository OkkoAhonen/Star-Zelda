using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopItemPriceScript : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI; // Viittaus TextMeshProUGUI-komponenttiin
    public int price = 50;                  // Hinta, asetetaan Inspectorissa
    public string text;                     // Lopullinen n�ytett�v� teksti (ei oletusteksti� t�ss�)

    void Start()
    {
        // Haetaan TextMeshProUGUI-komponentti, jos ei asetettu Inspectorissa
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        // Asetetaan teksti, jos komponentti l�ytyy
        if (textMeshProUGUI != null)
        {
            UpdatePriceText(); // P�ivitet��n teksti funktiolla
        }
        else
        {
            Debug.LogWarning("ShopItemPriceScript: TextMeshProUGUI-komponentti puuttuu!");
        }
    }

    // Funktio hinnan p�ivitt�miseen
    private void UpdatePriceText()
    {
        text = $"{price} $"; // Interpolointi: yhdist�� hinnan ja valuuttamerkin
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