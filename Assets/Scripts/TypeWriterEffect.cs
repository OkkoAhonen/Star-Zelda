using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    public float WriteSpeed = 20f;
    public Coroutine Run(string textToType, TMP_Text textlabel) 
    {
        return StartCoroutine(TypeText(textToType, textlabel));
    }

    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        textLabel.text = string.Empty;


        float t = 0;
        int CharIndex = 0;

        while (CharIndex < textToType.Length)
        {
            t += Time.deltaTime * WriteSpeed;
            CharIndex = Mathf.FloorToInt(t);
            CharIndex = Mathf.Clamp(CharIndex, 0, textToType.Length);

            textLabel.text = textToType.Substring(0, CharIndex);

            yield return null;
        }

        textLabel.text = textToType;
    }

}
