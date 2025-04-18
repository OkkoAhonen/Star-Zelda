using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class CreditEntry
{
    [TextArea(2, 4)]
    public string text; // The text content for this entry
    public float fontSize = 24f; // Font size for this entry
    public bool isBold = false; // Whether this entry is bold
    public float lineSpacing = 10f; // Extra spacing after this entry
}

public class Credits : MonoBehaviour
{
    [SerializeField] private Text creditsText; // UI Text component to display credits
    [SerializeField] private float scrollSpeed = 50f; // Speed of scrolling in pixels per second
    [SerializeField] private float endPositionY = 800f; // Y position where credits stop
    [SerializeField] private string mainMenuScene = "Credits"; // Name of main menu scene
    [SerializeField] private List<CreditEntry> creditEntries = new List<CreditEntry>(); // List of credit entries
    private RectTransform textTransform;
    private bool isScrolling = true;

    void Start()
    {
        textTransform = creditsText.GetComponent<RectTransform>();
        SetupCreditsText();
    }

    void Update()
    {
        if (isScrolling)
        {
            // Move text upward
            textTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // Check if credits have reached the end position
            if (textTransform.anchoredPosition.y >= endPositionY)
            {
                isScrolling = false;
                // Return to main menu after a delay
                Invoke("ReturnToMainMenu", 2f);
            }
        }

        // Allow manual skip with input (e.g., Escape key)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReturnToMainMenu();
        }
    }

    void SetupCreditsText()
    {
        // Build the credits string from the list of entries
        string creditsContent = "";
        foreach (var entry in creditEntries)
        {
            string formattedText = entry.text;
            // Apply bold formatting
            if (entry.isBold)
            {
                formattedText = $"<b>{formattedText}</b>";
            }
            // Apply font size
            formattedText = $"<size={entry.fontSize}>{formattedText}</size>";
            // Add the text and spacing
            creditsContent += formattedText + "\n";
            if (entry.lineSpacing > 0)
            {
                creditsContent += $"<size={entry.lineSpacing}>\n</size>";
            }
        }

        creditsText.text = creditsContent;
        creditsText.alignment = TextAnchor.MiddleCenter;
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}