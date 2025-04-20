using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemTextScript : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI; // Viittaus TextMeshProUGUI-komponenttiin
    public string text = "Oletusteksti";    // Oletusteksti, joka voidaan asettaa Inspectorissa

    private void Awake()
    {
        // Haetaan TextMeshProUGUI-komponentti, jos sitä ei ole asetettu Inspectorissa
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {


        // Asetetaan teksti, jos komponentti löytyy
        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.text = text;
        }
        else
        {
            Debug.LogWarning("ItemTextScript: TextMeshProUGUI-komponentti puuttuu!");
        }
    }

    void Update()
    {
        // Tyhjä, jos ei tarvita päivitystä joka frame
    }

    // Bonus: Funktio tekstin muuttamiseen myöhemmin
    public void SetText(string newText)
    {
        if (textMeshProUGUI != null)
        {
            text = newText;
            textMeshProUGUI.text = text;
        }
    }
}