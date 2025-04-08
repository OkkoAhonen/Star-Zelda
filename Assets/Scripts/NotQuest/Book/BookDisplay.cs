using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BookDisplay : MonoBehaviour
{
    [Header("Book Parts")]
    public GameObject frontCover;
    public GameObject backCover;
    public GameObject insideCover;
    public BookPagesContainer pagesContainer;
    public GameObject bookUI;

    [Header("UI Elements")]
    public TMP_InputField leftPageInput;
    public TMP_InputField rightPageInput;
    public TMP_Text pageNumberLeft;
    public TMP_Text pageNumberRight;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    private BookData currentBook => pagesContainer?.bookData;
    private int currentPage = 0;
    private bool isOpen = false;

    void Start()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(CloseBook);

        leftPageInput.onEndEdit.AddListener((text) => HandleTextInput(text, true));
        rightPageInput.onEndEdit.AddListener((text) => HandleTextInput(text, false));

        GameEventsManager.instance.inputEvents.onBookOpen += OpenBook;

        SetBookState(false);
    }

    private void SetBookState(bool open)
    {
        isOpen = open;
        
        frontCover.SetActive(!open);
        backCover.SetActive(!open);
        insideCover.SetActive(open);
        bookUI.SetActive(open);

        // Hide all pages when closing
        if (!open)
        {
            foreach (var page in currentBook.pages)
            {
                if (page.pageObject != null)
                {
                    page.pageObject.SetActive(false);
                }
            }
        }
        else
        {
            UpdatePages();
        }
    }

    public void OpenBook()
    {
        if (currentBook != null)
        {
            currentPage = 0;
            SetBookState(true);
        }
    }

    public void CloseBook()
    {
        SetBookState(false);
    }

    public void NextPage()
    {
        if (currentPage < currentBook.pages.Count - 1)
        {
            currentPage += 2;
            UpdatePages();
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

        int pageIndex = isLeftPage ? currentPage - 1 : currentPage;
        if (pageIndex >= 0 && pageIndex < currentBook.pages.Count)
        {
            currentBook.AddTextToCurrentPage(newText, pageIndex);
        }
    }

    public void UpdatePages()
    {
        if (currentBook == null || !isOpen) return;

        // Hide all pages
        foreach (var page in currentBook.pages)
        {
            if (page.pageObject != null)
            {
                page.pageObject.SetActive(false);
            }
        }

        // Handle first page (show only right page)
        if (currentPage == 0)
        {
            if (currentBook.pages.Count > 0)
            {
                currentBook.pages[0].pageObject?.SetActive(true);
                UpdatePageContent(rightPageInput, pageNumberRight, currentBook.pages[0], 1);
                leftPageInput.text = "";
                pageNumberLeft.text = "";
            }
        }
        // Handle last page for odd number of pages
        else if (currentPage == currentBook.pages.Count - 1 && currentBook.pages.Count % 2 != 0)
        {
            var lastPage = currentBook.pages[currentPage];
            lastPage.pageObject?.SetActive(true);
            UpdatePageContent(leftPageInput, pageNumberLeft, lastPage, currentPage + 1);
            rightPageInput.text = "";
            pageNumberRight.text = "";
        }
        // Handle normal two-page spread
        else if (currentPage < currentBook.pages.Count)
        {
            var leftPage = currentBook.pages[currentPage - 1];
            var rightPage = currentBook.pages[currentPage];
            
            leftPage.pageObject?.SetActive(true);
            rightPage.pageObject?.SetActive(true);
            
            UpdatePageContent(leftPageInput, pageNumberLeft, leftPage, currentPage);
            UpdatePageContent(rightPageInput, pageNumberRight, rightPage, currentPage + 1);
        }

        // Update navigation buttons
        nextButton.interactable = currentPage < currentBook.pages.Count - 1;
        prevButton.interactable = currentPage > 0;
    }

    private void UpdatePageContent(TMP_InputField input, TMP_Text pageNumber, BookPage page, int number)
    {
        input.text = page.text;
        input.interactable = currentBook.playerCanEdit && page.isEditable;
        pageNumber.text = number.ToString();
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(NextPage);
        prevButton.onClick.RemoveListener(PreviousPage);
        closeButton.onClick.RemoveListener(CloseBook);

        leftPageInput.onEndEdit.RemoveListener((text) => HandleTextInput(text, true));
        rightPageInput.onEndEdit.RemoveListener((text) => HandleTextInput(text, false));

        GameEventsManager.instance.inputEvents.onBookOpen -= OpenBook;
    }
}

