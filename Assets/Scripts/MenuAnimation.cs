using UnityEngine;
using UnityEngine.UI;
using TMPro; // Lis‰t‰‰n TextMeshPro-namespace

public class MenuAnimation : MonoBehaviour
{
    // Taulukko, johon lis‰‰t nappulat Unityn Inspectorissa
    public Button[] menuButtons;

    // Nappuloiden lopulliset X-positio (m‰‰rit‰ Inspectorissa)
    public Vector2[] targetPositions;

    // Pelin nimi TextMeshPro-elementtin‰
    public GameObject gameTitle;


    // Pelin nimen lopullinen Y-positio (m‰‰rit‰ Inspectorissa)
    public Vector2 titleTargetPosition;

    // Nopeus, jolla nappulat ja nimi liikkuvat
    public float moveSpeed = 5f;

    // Viive nappuloiden v‰lill‰ sekunneissa
    public float delayBetweenButtons = 0.2f;

    // Alkupositio nappuloille (ruudun vasemmasta reunasta ulkopuolella)
    private Vector2 buttonStartPosition = new Vector2(-200f, 0f);

    // Alkupositio nimelle (ruudun yl‰reunan ulkopuolella)
    private Vector2 titleStartPosition = new Vector2(0f, 600f);

    private bool[] hasMoved; // Seuraa, mitk‰ nappulat ovat liikkuneet
    private bool titleHasMoved; // Seuraa, onko nimi alkanut liikkua

    void Start()
    {
        // Varmista, ett‰ taulukot ovat samanpituiset
        if (menuButtons.Length != targetPositions.Length)
        {
            Debug.LogError("Nappuloiden ja kohdepositiotaulukon pituuden t‰ytyy olla sama!");
            return;
        }

        hasMoved = new bool[menuButtons.Length];

        // Aseta nappulat alkupositioon
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].GetComponent<RectTransform>().anchoredPosition = buttonStartPosition;
            hasMoved[i] = false;
        }

        // Aseta pelin nimi alkupositioon
        if (gameTitle != null)
        {
            gameTitle.GetComponent<RectTransform>().anchoredPosition = titleStartPosition;
            titleHasMoved = false;
        }
        else
        {
            Debug.LogWarning("GameTitle (TMP_Text) ei ole asetettu Inspectorissa!");
        }

        // K‰ynnist‰ animaatio
        StartCoroutine(AnimateMenu());
    }

    System.Collections.IEnumerator AnimateMenu()
    {
        // Aloita pelin nimen animaatio ensin
        if (gameTitle != null)
        {
            titleHasMoved = true;
            yield return new WaitForSeconds(delayBetweenButtons); // Pieni viive ennen nappuloita
        }

        // Animaatio nappuloille
        for (int i = 0; i < menuButtons.Length; i++)
        {
            yield return new WaitForSeconds(delayBetweenButtons); // Viive ennen seuraavaa nappulaa
            hasMoved[i] = true; // Merkitse nappula liikkeelle
        }
    }

    void Update()
    {
        // Liikuta nappuloita kohti niiden kohdepositiota
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (hasMoved[i])
            {
                RectTransform buttonTransform = menuButtons[i].GetComponent<RectTransform>();
                buttonTransform.anchoredPosition = Vector2.Lerp(
                    buttonTransform.anchoredPosition,
                    targetPositions[i],
                    Time.deltaTime * moveSpeed
                );

                // Pys‰yt‰ liike, kun nappula on tarpeeksi l‰hell‰ kohdetta
                if (Vector2.Distance(buttonTransform.anchoredPosition, targetPositions[i]) < 0.1f)
                {
                    buttonTransform.anchoredPosition = targetPositions[i];
                    hasMoved[i] = false; // Lopeta liikuttaminen
                }
            }
        }

        // Liikuta pelin nime‰ kohti sen kohdepositiota
        if (titleHasMoved && gameTitle != null)
        {
            RectTransform titleTransform = gameTitle.GetComponent<RectTransform>();
            titleTransform.anchoredPosition = Vector2.Lerp(
                titleTransform.anchoredPosition,
                titleTargetPosition,
                Time.deltaTime * moveSpeed
            );

            // Pys‰yt‰ liike, kun nimi on tarpeeksi l‰hell‰ kohdetta
            if (Vector2.Distance(titleTransform.anchoredPosition, titleTargetPosition) < 0.1f)
            {
                titleTransform.anchoredPosition = titleTargetPosition;
                titleHasMoved = false; // Lopeta liikuttaminen
            }
        }
    }
}