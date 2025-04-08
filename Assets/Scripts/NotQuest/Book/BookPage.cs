using UnityEngine;
using TMPro;

[System.Serializable]
public class BookPage
{
    [TextArea(5, 15)] public string text; // Text content for the page
    public bool isEditable; // Indicates if the page is editable
    public GameObject pageObject; // Reference to the physical page object
    
    // Calculate if text will overflow based on TMP text component
    public bool WillTextOverflow(TMP_Text textComponent)
    {
        textComponent.text = text;
        return textComponent.isTextOverflowing;
    }
}
