using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(fileName = "NewBook", menuName = "Book System/Book")]
public class BookData : ScriptableObject
{
    public string title;                     // Title of the book
    public bool playerCanEdit;               // Can the player edit this book?
    
    [Header("Content")]
    public List<BookPage> pages = new List<BookPage>(); // Pages of the book

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
            pages.Add(new BookPage { text = initialText }); // Create a new page
        }
    }
}
