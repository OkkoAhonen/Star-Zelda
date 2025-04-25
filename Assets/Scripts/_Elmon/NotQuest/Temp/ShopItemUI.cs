using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text descriptionText;
    public Image iconImage;
    public Button buyButton; // Slides
    public GameObject buyButtonEnd;
    public Button toggleButton; // Use as needed for additional UI effects

    [Header("Item Data")]
    public Perk perkData;  // Data about the perk to sell
    private int cost;

    [Header("Slide Settings")]
    public float slideDuration = 0.3f;

    private bool buttonIsOpen = false;
    private Vector2 closedPos;
    private Vector2 openPos;
    private Coroutine slideCoroutine;

    // Initializes the shop item UI with perk data.
    public void Initialize(Perk perk)
    {
        perkData = perk;
        cost = perk.price;
        nameText.text = perk.perkName;
        priceText.text = cost.ToString();
        descriptionText.text = perk.description;
        iconImage.sprite = perk.icon;
        buyButton.onClick.AddListener(Buy);
        toggleButton.onClick.AddListener(ToggleItem);
    }

    // Expose cost for sorting.
    public int Cost { get { return cost; } }

    private void Start()
    {
        // Set initial positions based on the current button position.
        closedPos = buyButton.GetComponent<RectTransform>().anchoredPosition;
        openPos = buyButtonEnd.GetComponent<RectTransform>().anchoredPosition;

        // Start closed.
        buyButton.GetComponent<RectTransform>().anchoredPosition = closedPos;
        buyButton.interactable = false;
        gameObject.SetActive(true);
    }

    // Called when the buy button is clicked.
    public void Buy()
    {
        ShopTemp shop = (ShopTemp)FindFirstObjectByType(typeof(ShopTemp));
        
        if (!shop.availablePerks.Contains(perkData))
        {
            Debug.LogWarning("Perk not sold here.");
            return;
        }
        PerkSystem perkSystem = PerkSystem.instance;
        if (!perkSystem.TryUnlockPerk(perkData))
            return;
        if (PlayerStatsManager.instance.CurrentGold < cost)
        {
            Debug.LogWarning("Not enough gold to buy perk.");
            return;
        }
        GameEventsManager.instance.playerEvents.ChangeGoldBy(-cost);
        shop.unlockedPerks.Add(perkData);
        Debug.Log("Purchased perk: " + perkData.perkName);
        shop.ReorderItems();

        // Disable all relevant buttons after purchase
        buyButton.interactable = false;
        toggleButton.interactable = false;
        buyButton.gameObject.SetActive(false);
        toggleButton.gameObject.SetActive(false);
        buyButtonEnd.SetActive(false);

        // Fade them out
        StartCoroutine(FadeOut(gameObject));
    }
    
    private IEnumerator FadeOut(GameObject someObject)
    {
        CanvasGroup canvasGroup = someObject.GetComponent<CanvasGroup>();

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0.45f, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 0.3f;
    }

    // Call to toggle the sliding of the buy button.
    public void ToggleItem()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlidePanel(buttonIsOpen ? openPos : closedPos, buttonIsOpen ? closedPos : openPos));
        buttonIsOpen = !buttonIsOpen;
        buyButton.interactable = !buyButton.interactable;
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        RectTransform rect = buyButton.GetComponent<RectTransform>();

        while (elapsed < slideDuration)
        {
            rect.anchoredPosition = Vector2.Lerp(from, to, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = to;
    }


    private void OnDestroy()
    {
        buyButton.onClick.RemoveAllListeners();
    }
}
