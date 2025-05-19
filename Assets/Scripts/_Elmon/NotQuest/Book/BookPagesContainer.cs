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
            CachePages();
            // Hide all pages after initialization
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(false);
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