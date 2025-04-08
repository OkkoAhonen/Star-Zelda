using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(fileName = "NewBook", menuName = "Book System/Book")]
public class BookData : ScriptableObject
{
    [Header("Book Info")]
    public string title;                     // Title of the book
    public bool playerCanEdit;               // Can the player edit this book?
    public string creator;
    public string creationDate;
    
    [Header("Content")]
    public List<BookPage> pages = new List<BookPage>(); // Pages of the book

    public void InitializePagesWithObjects(List<GameObject> pageObjects)
    {
        pages.Clear();
        foreach (var pageObj in pageObjects)
        {
            pages.Add(new BookPage 
            { 
                text = "", 
                isEditable = playerCanEdit,
                pageObject = pageObj
            });
        }
    }

    public void InitializePages(int numberOfPages)
    {
        pages.Clear();
        // Ensure we have an even number of pages
        if (numberOfPages % 2 != 0) numberOfPages++;
        
        for (int i = 0; i < numberOfPages; i++)
        {
            pages.Add(new BookPage { text = "", isEditable = playerCanEdit });
        }
    }

    public void AddTextToCurrentPage(string newText, int currentPage)
    {
        if (playerCanEdit && currentPage < pages.Count)
        {
            pages[currentPage].text += newText; // Append text to the current page
        }
    }

    public void CreateNewPage(string initialText = "")
    {
        if (playerCanEdit)
        {
            pages.Add(new BookPage { text = initialText, isEditable = true });
            // Add a second page to maintain pairs
            pages.Add(new BookPage { text = "", isEditable = true });
        }
    }
}
