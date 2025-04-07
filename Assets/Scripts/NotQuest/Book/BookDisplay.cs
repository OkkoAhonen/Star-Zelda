using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject bookUI;               // Parent UI GameObject to enable/disable
    public TMP_Text leftPageText;           // Text content for the left page
    public TMP_Text rightPageText;          // Text content for the right page
    public TMP_Text leftPageNumberText;     // Page number for the left page
    public TMP_Text rightPageNumberText;    // Page number for the right page

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("Current Book")]
    public BaseBook currentBook;

    [Header("Input Settings")]
    public TMP_InputField editableTextField; // Only active when book is editable

    private int currentPage = 0; // Start at the first page

    void Start()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(CloseBook);

        GameEventsManager.instance.inputEvents.onBookOpen += OpenBook;

        ToggleBook(false);

        if (editableTextField != null)
        {
            editableTextField.onValueChanged.AddListener(HandleTextInput);
        }
    }

    public void OpenBook()
    {
        if (currentBook != null)
        {
            currentPage = 0; // Reset to the first page when opening the book
            ToggleBook(true);
            UpdatePage();
        }
    }

    public void CloseBook()
    {
        ToggleBook(false);
    }

    void ToggleBook(bool open)
    {
        bookUI.SetActive(open);
    }

    public void NextPage()
    {
        if (currentPage < currentBook.pages.Count - 1) // Check for at least one page
        {
            currentPage++; // Move to the next page
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--; // Move back to the previous page
            UpdatePage();
        }
    }

    private void HandleTextInput(string newText)
    {
        if (currentBook != null && currentBook.playerCanEdit)
        {
            currentBook.AddTextToCurrentPage(newText, currentPage);
            UpdatePage();
        }
    }

    public void UpdatePage()
    {
        if (currentBook == null || currentBook.pages.Count == 0) return;

        // Set text for the right page (current page)
        if (currentPage < currentBook.pages.Count)
        {
            BookPage rightPage = currentBook.pages[currentPage];
            rightPageText.text = rightPage.text;
            rightPageNumberText.text = (currentPage + 1).ToString(); // Set right page number
        }
        else
        {
            rightPageText.text = ""; // Clear if no page exists
            rightPageNumberText.text = ""; // Clear if no page exists
        }

        // Set text for the left page (previous page)
        if (currentPage - 1 >= 0)
        {
            BookPage leftPage = currentBook.pages[currentPage - 1];
            leftPageText.text = leftPage.text;
            leftPageNumberText.text = (currentPage).ToString(); // Set left page number
        }
        else
        {
            leftPageText.text = ""; // Clear if no page exists
            leftPageNumberText.text = ""; // Clear if no page exists
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        nextButton.onClick.RemoveListener(NextPage);
        prevButton.onClick.RemoveListener(PreviousPage);
        closeButton.onClick.RemoveListener(CloseBook);

        // Unsubscribe from the book open event
        GameEventsManager.instance.inputEvents.onBookOpen -= OpenBook;
    }
}
