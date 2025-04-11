using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BookDisplay : MonoBehaviour
{
    [Header("Default Book")]
    public BookData defaultBook; // Notebook in inspector

    [Header("Book Parts")]
    public GameObject frontCover;
    public GameObject backCover;
    public GameObject insideCover;
    public GameObject leftPage;
    public GameObject rightPage;
    public GameObject middleMarker;
    public GameObject bookUI;
    public Transform bookContentParent; // Where book prefabs will be instantiated

    [Header("UI Elements")]
    public TMP_InputField creator;
    public TMP_InputField creationDate;
    public TMP_InputField leftPageInput;
    public TMP_InputField rightPageInput;
    public TMP_Text pageNumberLeft;
    public TMP_Text pageNumberRight;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button openButton;
    public Button closeButton;
    public Button refreshButton;

    [Header("Page Layout")]
    public bool startOnRightPage = true; // If true, first page is on right, then left-right pairs

    [Header("Page Positions")]
    private Vector3 leftPagePosition;
    private Vector3 rightPagePosition;

    private BookPagesContainer currentBookContainer;
    private BookData currentBook => currentBookContainer?.bookData;
    private int currentPage = 0;
    private bool isOpen = false;

    void Start()
    {
        leftPagePosition = leftPage.transform.position;
        rightPagePosition = rightPage.transform.position;
        
        // Only set current book if there's no content
        if (bookContentParent.childCount == 0)
        {
            SetCurrentBook(defaultBook);
        }
        else
        {
            // Use existing notebook
            currentBookContainer = bookContentParent.GetChild(0).GetComponent<BookPagesContainer>();
        }
        
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        openButton.onClick.AddListener(OpenBook);
        closeButton.onClick.AddListener(CloseBook);
        refreshButton.onClick.AddListener(RefreshBook);

        leftPageInput.onEndEdit.AddListener((text) => HandleTextInput(text, true));
        rightPageInput.onEndEdit.AddListener((text) => HandleTextInput(text, false));

        GameEventsManager.instance.inputEvents.onBookToggle += ToggleBook;

        // Show the book to hide it
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        
        // Start with book hidden
        gameObject.SetActive(false);
        // But initialize it as closed
        SetBookState(false);
    }

    private void ToggleBook()
    {
        // Toggle the entire book on/off
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
        {
            RefreshBook();
        }
    }

    public void SetCurrentBook(BookData newBook)
    {
        // Destroy current book if exists
        if (currentBookContainer != null)
        {
            Destroy(currentBookContainer.gameObject);
        }

        // Create new book from prefab
        GameObject bookPrefab = newBook.bookPrefab; // Add this field to BookData
        if (bookPrefab != null)
        {
            GameObject bookInstance = Instantiate(bookPrefab, bookContentParent);
            currentBookContainer = bookInstance.GetComponent<BookPagesContainer>();
            currentBookContainer.bookData = newBook;
        }
    }

    private void SetBookState(bool open)
    {
        isOpen = open;
        
        // Show/hide covers and pages
        frontCover.SetActive(!open);
        backCover.SetActive(!open);
        insideCover.SetActive(open);
        leftPage.SetActive(open);
        rightPage.SetActive(open);
        middleMarker.SetActive(open);

        // Show/hide book content and UI
        bookContentParent.gameObject.SetActive(true); // Always keep content parent active
        bookUI.SetActive(true);
        openButton.gameObject.SetActive(!open);
        closeButton.gameObject.SetActive(open);
        nextButton.gameObject.SetActive(open);
        prevButton.gameObject.SetActive(open);
        leftPageInput.gameObject.SetActive(open);
        rightPageInput.gameObject.SetActive(open);
        pageNumberLeft.gameObject.SetActive(open);
        pageNumberRight.gameObject.SetActive(open);

        if (open)
        {
            UpdatePages();
        }
        else
        {
            // Hide all pages in the book content
            if (currentBookContainer != null)
            {
                for (int i = 0; i < currentBookContainer.PageCount; i++)
                {
                    currentBookContainer.GetPage(i)?.SetActive(false);
                }
            }
        }
    }

    public void OpenBook()
    {
        // Just open the book (hide covers, show pages)
        if (currentBook != null)
        {
            currentPage = 0;
            SetBookState(true);
        }
    }

    public void CloseBook()
    {
        // Just close the book (show covers, hide pages)
        SetBookState(false);
    }

    public void NextPage()
    {
        if (startOnRightPage)
        {
            if (currentPage < currentBookContainer.PageCount - 1)
            {
                currentPage += 2;
                UpdatePages();
            }
        }
        else
        {
            if (currentPage < currentBookContainer.PageCount - 2)
            {
                currentPage += 2;
                UpdatePages();
            }
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage -= 2;
            UpdatePages();
        }
    }

    private void HandleTextInput(string newText, bool isLeftPage)
    {
        if (currentBook == null || !currentBook.playerCanEdit) return;
        // Handle text input directly on the page GameObject if needed
    }

    public void UpdatePages()
    {
        if (currentBook == null || !isOpen) return;

        // Hide all pages and clear numbers
        for (int i = 0; i < currentBookContainer.PageCount; i++)
        {
            currentBookContainer.GetPage(i)?.SetActive(false);
        }
        pageNumberLeft.text = "";
        pageNumberRight.text = "";

        if (startOnRightPage)
        {
            // First page is on right
            if (currentPage == 0)
            {
                var page = currentBookContainer.GetPage(0);
                if (page != null)
                {
                    page.SetActive(true);
                    page.transform.position = rightPagePosition;
                    pageNumberRight.text = "1";
                }
            }
            // Normal left-right pairs
            else
            {
                var leftPage = currentBookContainer.GetPage(currentPage - 1);
                var rightPage = currentBookContainer.GetPage(currentPage);
                
                if (leftPage != null)
                {
                    leftPage.SetActive(true);
                    leftPage.transform.position = leftPagePosition;
                    pageNumberLeft.text = currentPage.ToString();
                }
                if (rightPage != null)
                {
                    rightPage.SetActive(true);
                    rightPage.transform.position = rightPagePosition;
                    pageNumberRight.text = (currentPage + 1).ToString();
                }
            }
        }
        else
        {
            // When startOnRightPage is false, always show both pages
            var leftPage = currentBookContainer.GetPage(currentPage);
            var rightPage = currentBookContainer.GetPage(currentPage + 1);
            
            if (leftPage != null)
            {
                leftPage.SetActive(true);
                leftPage.transform.position = leftPagePosition;
                pageNumberLeft.text = (currentPage + 1).ToString();
            }
            if (rightPage != null)
            {
                rightPage.SetActive(true);
                rightPage.transform.position = rightPagePosition;
                pageNumberRight.text = (currentPage + 2).ToString();
            }
        }

        // Update button interactability
        if (startOnRightPage)
        {
            nextButton.interactable = currentPage < currentBookContainer.PageCount - 1;
        }
        else
        {
            nextButton.interactable = currentPage < currentBookContainer.PageCount - 2;
        }
        prevButton.interactable = currentPage > 0;
    }

    public void RefreshBook()
    { // Refresh something on book, even while it's open
        if (currentBook == null) return;

        // Later you might "rewrite" stats, "refreshing" them

        // Refresh page numbers
        UpdatePages();
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(NextPage);
        prevButton.onClick.RemoveListener(PreviousPage);
        openButton.onClick.RemoveListener(OpenBook);
        closeButton.onClick.RemoveListener(CloseBook);
        refreshButton.onClick.RemoveListener(RefreshBook);

        leftPageInput.onEndEdit.RemoveListener((text) => HandleTextInput(text, true));
        rightPageInput.onEndEdit.RemoveListener((text) => HandleTextInput(text, false));

        GameEventsManager.instance.inputEvents.onBookToggle -= ToggleBook;
    }
}

