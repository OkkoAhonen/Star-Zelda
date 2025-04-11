using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BookPagesContainer : MonoBehaviour
{
    [Header("Book Data")]
    public BookData bookData;

    private BookDisplay bookDisplay;
    private List<GameObject> pages = new List<GameObject>();

    void Start()
    {
        // Get the BookDisplay from parent
        bookDisplay = GetComponentInParent<BookDisplay>();
        if (bookDisplay != null)
        {
            InitializeBookInfo();
            CachePages();
            // Hide all pages after initialization
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(false);
            }
        }
    }

    private void InitializeBookInfo()
    {
        if (bookData != null && bookDisplay != null)
        {
            // Update title if component exists
            TMP_InputField titleText = bookDisplay.frontCover?.transform.GetComponent<TMP_InputField>();
            if (titleText != null)
            {
                titleText.text = bookData.title;
            }

            // Update creator and creation date using references from BookDisplay
            if (bookDisplay.creator != null)
            {
                bookDisplay.creator.text = bookData.creator;
            }
            if (bookDisplay.creationDate != null)
            {
                bookDisplay.creationDate.text = bookData.creationDate;
            }
        }
    }

    private void CachePages()
    {
        pages.Clear();
        foreach (Transform child in transform)
        {
            pages.Add(child.gameObject);
        }
    }

    public GameObject GetPage(int index)
    {
        if (index >= 0 && index < pages.Count)
            return pages[index];
        return null;
    }

    public int PageCount => pages.Count;
} 