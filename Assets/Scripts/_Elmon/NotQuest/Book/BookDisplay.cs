using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BookDisplay : MonoBehaviour
{
    [Header("Default Book")]
    public BookData defaultBook;

    [Header("Sprites")]
    public Sprite openSprite;
    public Sprite defaultSprite;

    [Header("Cover Image")]
    public Image image;

    [Header("Book Parts")]
    public GameObject bookUI;
    public GameObject bookContentParent;

    [Header("UI Elements")]
    public TMP_Text pageNumberLeft;
    public TMP_Text pageNumberRight;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button openButton;
    public Button closeButton;

    [Header("Page Layout")]
    public bool startOnRightPage = true;

    [Header("Animation Settings")]
    [SerializeField] private float offScreenOffset = 1000f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Animator Branching")]
    public bool closeBookNormal; // choose your open/close animation variant

    [Header("Information (Debug)")]
    [SerializeField] private int currentPage;
    [SerializeField] private bool bookOpen;
    [SerializeField] private bool bookUp;
    [SerializeField] private BookPagesContainer currentBookContainer;

    private enum AnimationType { None, Open, Close, Flip }
    private AnimationType lastAnimation = AnimationType.None;

    private RectTransform bookRectTransform;
    private Vector2 bookOnScreenPos;
    private Vector2 bookOffScreenPos;
    private Animator animator;

    private BookData CurrentBook => currentBookContainer?.bookData;

    void Start()
    {
        animator = GetComponent<Animator>();
        bookRectTransform = GetComponent<RectTransform>();

        // Cover icon starts closed
        if (image != null)
            image.sprite = defaultSprite;

        // Off-screen setup
        bookOnScreenPos = bookRectTransform.anchoredPosition;
        bookOffScreenPos = bookOnScreenPos + Vector2.right * offScreenOffset;
        bookRectTransform.anchoredPosition = bookOffScreenPos;
        gameObject.SetActive(false);

        // Load or instantiate pages
        if (bookContentParent.transform.childCount == 0)
            SetCurrentBook(defaultBook);
        else
            currentBookContainer = bookContentParent.transform
                                    .GetChild(0)
                                    .GetComponent<BookPagesContainer>();

        // Wire buttons
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        openButton.onClick.AddListener(OpenBook);
        closeButton.onClick.AddListener(CloseBook);

        // Listen for toggle event
        GameEventsManager.instance.inputEvents.onBookToggle += ToggleBook;

        // Start closed
        bookOpen = false;
        bookUp = false;
        RefreshButtons();
        animator.enabled = false;
    }

    private void SetCurrentBook(BookData data)
    {
        if (currentBookContainer != null)
            Destroy(currentBookContainer.gameObject);

        if (data.bookPrefab != null)
        {
            var inst = Instantiate(data.bookPrefab, bookContentParent.transform);
            currentBookContainer = inst.GetComponent<BookPagesContainer>();
            currentBookContainer.bookData = data;
        }
    }

    private void RefreshButtons()
    {
        // Entire GameObjects on/off
        openButton.gameObject.SetActive(!bookOpen);
        closeButton.gameObject.SetActive(bookOpen);
        prevButton.gameObject.SetActive(bookOpen);
        nextButton.gameObject.SetActive(bookOpen);
    }

    private void UpdatePages()
    {
        if (!bookOpen || currentBookContainer == null) return;

        // Hide all pages
        for (int i = 0; i < currentBookContainer.PageCount; i++)
            currentBookContainer.GetPage(i)?.SetActive(false);

        pageNumberLeft.text = "";
        pageNumberRight.text = "";

        if (startOnRightPage)
        {
            if (currentPage == 0)
            {
                var p = currentBookContainer.GetPage(0);
                if (p != null) p.SetActive(true);
                pageNumberRight.text = "1";
            }
            else
            {
                var left = currentBookContainer.GetPage(currentPage - 1);
                var right = currentBookContainer.GetPage(currentPage);
                if (left != null) left.SetActive(true);
                if (right != null) right.SetActive(true);
                pageNumberLeft.text = currentPage.ToString();
                pageNumberRight.text = (currentPage + 1).ToString();
            }
        }
        else
        {
            var left = currentBookContainer.GetPage(currentPage);
            var right = currentBookContainer.GetPage(currentPage + 1);
            if (left != null) left.SetActive(true);
            if (right != null) right.SetActive(true);
            pageNumberLeft.text = (currentPage + 1).ToString();
            pageNumberRight.text = (currentPage + 2).ToString();
        }
    }

    private IEnumerator PositionAnimation()
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float elapsed = 0f;
        Vector2 from = bookUp ? bookOnScreenPos : bookOffScreenPos;
        Vector2 to = bookUp ? bookOffScreenPos : bookOnScreenPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            bookRectTransform.anchoredPosition =
                Vector2.LerpUnclamped(from, to, animationCurve.Evaluate(t));
            yield return null;
        }
        bookRectTransform.anchoredPosition = to;

        if (bookUp)
        {
            bookUp = false;
            gameObject.SetActive(false);
        }
        else
        {
            bookUp = true;
        }
    }

    public void ToggleBook()
    {
        if (!bookUp)
            gameObject.SetActive(true);
        StartCoroutine(PositionAnimation());
    }

    public void OpenBook()
    {
        if (CurrentBook == null) return;
        lastAnimation = AnimationType.Open;
        currentPage = 0;
        StartCoroutine(PlayAndWait("open"));
    }

    public void CloseBook()
    {
        lastAnimation = AnimationType.Close;
        StartCoroutine(PlayAndWait("close"));
    }

    public void NextPage()
    {
        if (!bookOpen) return;
        int limit = startOnRightPage
            ? currentBookContainer.PageCount - 1
            : currentBookContainer.PageCount - 2;
        if (currentPage >= limit) return;

        currentPage += 2;
        lastAnimation = AnimationType.Flip;
        StartCoroutine(PlayAndWait("left", Random.Range(1, 4)));
    }

    public void PreviousPage()
    {
        if (!bookOpen || currentPage <= 0) return;
        currentPage -= 2;
        lastAnimation = AnimationType.Flip;
        StartCoroutine(PlayAndWait("right", Random.Range(1, 4)));
    }

    private IEnumerator PlayAndWait(string trigger, int randomInt = -1)
    {
        // hide content immediately
        bookUI.SetActive(false);
        bookContentParent.SetActive(false);

        animator.enabled = true;
        if (randomInt > 0)
            animator.SetInteger("random", randomInt);
        animator.SetBool("closeBookNormal", closeBookNormal);
        animator.SetTrigger(trigger);

        // wait one frame so clip info is valid
        yield return null;

        var clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
            yield return new WaitForSeconds(clips[0].clip.length - 0.05f);

        // handle result
        if (lastAnimation == AnimationType.Open)
        {
            bookOpen = true;
            if (image != null) image.sprite = openSprite;
            // show pages on first open
            UpdatePages();
        }
        else if (lastAnimation == AnimationType.Close)
        {
            bookOpen = false;
            if (image != null) image.sprite = defaultSprite;
        }
        else if (lastAnimation == AnimationType.Flip && bookOpen)
        {
            UpdatePages();
        }

        // reveal or hide UI/content
        bookUI.SetActive(bookOpen);
        bookContentParent.SetActive(bookOpen);

        // refresh which buttons are present
        RefreshButtons();

        animator.enabled = false;
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(NextPage);
        prevButton.onClick.RemoveListener(PreviousPage);
        openButton.onClick.RemoveListener(OpenBook);
        closeButton.onClick.RemoveListener(CloseBook);
        GameEventsManager.instance.inputEvents.onBookToggle -= ToggleBook;
    }
}
