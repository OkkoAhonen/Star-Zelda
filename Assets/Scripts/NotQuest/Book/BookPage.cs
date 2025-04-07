using UnityEngine;
using TMPro;

[System.Serializable]
public class BookPage
{
    [TextArea(5, 15)] public string text;
    
    // Calculate if text will overflow based on TMP text component
    public bool WillTextOverflow(TMP_Text textComponent)
    {
        textComponent.text = text;
        return textComponent.isTextOverflowing;
    }
}
