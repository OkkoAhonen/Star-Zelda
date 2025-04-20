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
        // Haetaan TextMeshProUGUI-komponentti, jos sit� ei ole asetettu Inspectorissa
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {


        // Asetetaan teksti, jos komponentti l�ytyy
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
        // Tyhj�, jos ei tarvita p�ivityst� joka frame
    }

    // Bonus: Funktio tekstin muuttamiseen my�hemmin
    public void SetText(string newText)
    {
        if (textMeshProUGUI != null)
        {
            text = newText;
            textMeshProUGUI.text = text;
        }
    }
}