using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUIAnimation : MonoBehaviour
{
    // UI-elementit, jotka animoidaan (asetetaan Inspectorissa)
    public RectTransform inventoryPanel; // Inventory-paneeli
    public TMP_Text moneyText;          // Pelaajan raham‰‰r‰ (TextMeshPro)
    public Slider damageSlider;         // Damage-slider

    // Kohdepositiot UI-elementeille (asetetaan Inspectorissa)
    public Vector2 inventoryTargetPosition;
    public Vector2 moneyTextTargetPosition;
    public Vector2 damageSliderTargetPosition;

    // Nopeus, jolla elementit liikkuvat
    public float moveSpeed = 5f;

    // Viive elementtien animaatioiden v‰lill‰ sekunneissa
    public float delayBetweenElements = 0.2f;

    // Alkupositiot (ruudun ulkopuolella)
    private Vector2 inventoryStartPosition = new Vector2(-1500f, 0f);   // Vasemmalta
    private Vector2 moneyTextStartPosition = new Vector2(0f, 1000f);     // Ylh‰‰lt‰
    private Vector2 damageSliderStartPosition = new Vector2(-1000f, 0f);  // Oikealta

    // Seuraa, mitk‰ elementit ovat liikkeell‰
    private bool inventoryMoving;
    private bool moneyTextMoving;
    private bool damageSliderMoving;

    void Start()
    {
        // Aseta alkupositiot
        if (inventoryPanel != null)
        {
            inventoryPanel.anchoredPosition = inventoryStartPosition;
            inventoryMoving = false;
        }
        else
        {
            Debug.LogWarning("InventoryPanel ei ole asetettu Inspectorissa!");
        }

        if (moneyText != null)
        {
            moneyText.GetComponent<RectTransform>().anchoredPosition = moneyTextStartPosition;
            moneyTextMoving = false;
        }
        else
        {
            Debug.LogWarning("MoneyText (TMP_Text) ei ole asetettu Inspectorissa!");
        }

        if (damageSlider != null)
        {
            damageSlider.GetComponent<RectTransform>().anchoredPosition = damageSliderStartPosition;
            damageSliderMoving = false;
        }
        else
        {
            Debug.LogWarning("DamageSlider ei ole asetettu Inspectorissa!");
        }

        // K‰ynnist‰ animaatio sekunnin viiveell‰
        StartCoroutine(AnimateUI());
    }

    IEnumerator AnimateUI()
    {
        // Odota 1 sekunti ennen kuin aloitetaan ensimm‰inen animaatio
        yield return new WaitForSeconds(1f);

        // Animaatio inventorylle
        if (inventoryPanel != null)
        {
            inventoryMoving = true;
            yield return new WaitForSeconds(delayBetweenElements);
        }

        // Animaatio raham‰‰r‰lle
        if (moneyText != null)
        {
            moneyTextMoving = true;
            yield return new WaitForSeconds(delayBetweenElements);
        }

        // Animaatio damage-sliderille
        if (damageSlider != null)
        {
            damageSliderMoving = true;
        }
    }

    void Update()
    {
        // Liikuta inventory‰
        if (inventoryMoving && inventoryPanel != null)
        {
            RectTransform inventoryTransform = inventoryPanel;
            inventoryTransform.anchoredPosition = Vector2.Lerp(
                inventoryTransform.anchoredPosition,
                inventoryTargetPosition,
                Time.deltaTime * moveSpeed
            );

            // Pys‰yt‰, kun tarpeeksi l‰hell‰
            if (Vector2.Distance(inventoryTransform.anchoredPosition, inventoryTargetPosition) < 0.1f)
            {
                inventoryTransform.anchoredPosition = inventoryTargetPosition;
                inventoryMoving = false;
            }
        }

        // Liikuta raham‰‰r‰‰
        if (moneyTextMoving && moneyText != null)
        {
            RectTransform moneyTransform = moneyText.GetComponent<RectTransform>();
            moneyTransform.anchoredPosition = Vector2.Lerp(
                moneyTransform.anchoredPosition,
                moneyTextTargetPosition,
                Time.deltaTime * moveSpeed
            );

            // Pys‰yt‰, kun tarpeeksi l‰hell‰
            if (Vector2.Distance(moneyTransform.anchoredPosition, moneyTextTargetPosition) < 0.1f)
            {
                moneyTransform.anchoredPosition = moneyTextTargetPosition;
                moneyTextMoving = false;
            }
        }

        // Liikuta damage-slideri‰
        if (damageSliderMoving && damageSlider != null)
        {
            RectTransform sliderTransform = damageSlider.GetComponent<RectTransform>();
            sliderTransform.anchoredPosition = Vector2.Lerp(
                sliderTransform.anchoredPosition,
                damageSliderTargetPosition,
                Time.deltaTime * moveSpeed
            );

            // Pys‰yt‰, kun tarpeeksi l‰hell‰
            if (Vector2.Distance(sliderTransform.anchoredPosition, damageSliderTargetPosition) < 0.1f)
            {
                sliderTransform.anchoredPosition = damageSliderTargetPosition;
                damageSliderMoving = false;
            }
        }
    }
}