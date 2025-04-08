using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BookPagesContainer : MonoBehaviour
{
    [Header("Book Data")]
    public BookData bookData;

    [Header("UI References")]
    public TMP_Text titleText;        // Reference to Title under FrontCover
    public TMP_Text creatorText;      // Reference to Creator under InsideCover
    public TMP_Text creationDateText; // Reference to CreationDate under InsideCover

    void Start()
    {
        InitializeBookInfo();
    }

    private void InitializeBookInfo()
    {
        if (bookData != null)
        {
            // Update title if component exists
            if (titleText != null)
            {
                titleText.text = bookData.title;
            }

            // Update creator if component exists
            if (creatorText != null)
            {
                creatorText.text = bookData.creator;
            }

            // Update creation date if component exists
            if (creationDateText != null)
            {
                creationDateText.text = bookData.creationDate;
            }

            // Initialize pages with the physical page objects
            List<GameObject> pageObjects = new List<GameObject>();
            foreach (Transform child in transform)
            {
                pageObjects.Add(child.gameObject);
            }

            // If bookData doesn't have pages initialized yet, do it now
            if (bookData.pages.Count == 0 && pageObjects.Count > 0)
            {
                for (int i = 0; i < pageObjects.Count; i++)
                {
                    bookData.pages.Add(new BookPage 
                    { 
                        text = "",
                        isEditable = bookData.playerCanEdit,
                        pageObject = pageObjects[i]
                    });
                }
            }
            // If pages are already initialized, just update the page object references
            else if (bookData.pages.Count > 0)
            {
                for (int i = 0; i < Mathf.Min(bookData.pages.Count, pageObjects.Count); i++)
                {
                    bookData.pages[i].pageObject = pageObjects[i];
                }
            }
        }
    }
} 