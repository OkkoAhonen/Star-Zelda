using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    [System.Serializable]
    public class Page
    {
        [TextArea(3, 10)]
        public string content;
        public Sprite illustration; // Optional image for the page
    }

    [Header("Book Content")]
    [SerializeField] private Page[] pages;
    [SerializeField] private int currentPage = 0;

    [Header("UI References")]
    [SerializeField] private TMP_Text pageContent;
    [SerializeField] private TMP_Text pageNumber;
    [SerializeField] private Image illustration;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private CanvasGroup bookCanvasGroup;

    private void Start()
    {
        // Set up button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);

        // Initialize the book
        UpdatePageButtons();
        DisplayCurrentPage();
    }

    public void OpenBook()
    {
        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 1;
            bookCanvasGroup.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
        currentPage = 0;
        DisplayCurrentPage();
    }

    public void CloseBook()
    {
        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 0;
            bookCanvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            DisplayCurrentPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
        }
    }

    private void DisplayCurrentPage()
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("No pages assigned to the book!");
            return;
        }

        // Update text content
        if (pageContent != null)
            pageContent.text = pages[currentPage].content;

        // Update page number
        if (pageNumber != null)
            pageNumber.text = $"Page {currentPage + 1} of {pages.Length}";

        // Update illustration if there is one
        if (illustration != null)
        {
            if (pages[currentPage].illustration != null)
            {
                illustration.sprite = pages[currentPage].illustration;
                illustration.gameObject.SetActive(true);
            }
            else
            {
                illustration.gameObject.SetActive(false);
            }
        }

        UpdatePageButtons();
    }

    private void UpdatePageButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentPage > 0;
        
        if (nextButton != null)
            nextButton.interactable = currentPage < pages.Length - 1;
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);
        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);
    }
} 