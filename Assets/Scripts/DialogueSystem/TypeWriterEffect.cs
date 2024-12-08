using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    public float WriteSpeed = 20f;


    public bool IsRunning {  get; private set; }

    private readonly Dictionary<HashSet<char>, float> punktuations = new Dictionary<HashSet<char>, float>()
    {
        {new HashSet<char>{'.', '!', '?'}, 0.6f  },
        {new HashSet<char>{',', ':', ';'}, 0.3f  },
    };

    private Coroutine typingCoroutine;

    public void Run(string textToType, TMP_Text textLabel)
    {
        typingCoroutine = StartCoroutine(TypeText(textToType, textLabel));
    }

    public void Stop()
    {
        StopCoroutine(typingCoroutine);
        IsRunning = false;
    }

    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        IsRunning = true;
        textLabel.text = string.Empty;

        float t = 0;
        int charIndex = 0;

        while (charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * WriteSpeed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            for (int i = lastCharIndex; i < charIndex; i++)
            {
                if (i >= textToType.Length)
                    break; // Estetään ylivuoto

                bool isLast = i >= textToType.Length - 1;

                textLabel.text = textToType.Substring(0, i + 1);

                if (IsPunctuation(textToType[i], out float waitTime) && !isLast && i + 1 < textToType.Length && !IsPunctuation(textToType[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            yield return null;
        }


        IsRunning = false;
        
    }

    private bool IsPunctuation(char ch, out float waitTime)
    {
        foreach (KeyValuePair<HashSet<char>, float> punctuationCategory in punktuations)
        {
            if (punctuationCategory.Key.Contains(ch))
            {
                waitTime = punctuationCategory.Value;
                return true;
            }
        }
        waitTime = default;
        return false;
    }
}
