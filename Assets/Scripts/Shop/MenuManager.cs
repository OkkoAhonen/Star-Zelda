using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI-elementtej‰ varten

public class MenuManager : MonoBehaviour
{
    // Viittaus mustaan paneeliin, joka feidaa
    public Image fadePanel;

    // Nopeus, jolla feidaus tapahtuu
    public float fadeSpeed = 2f;

    // Paneelin alkupositio (vasemmalla ruudun ulkopuolella)
    private Vector2 startPosition = new Vector2(-1920f, 0f); // Oletetaan 1920x1080 resoluutio
    private Vector2 endPosition = new Vector2(0f, 0f); // Lopullinen positio peitt‰‰ ruudun

    void Start()
    {
        // Aseta paneeli alkupositioon ja varmista, ett‰ se on l‰pin‰kyv‰ aluksi
        if (fadePanel != null)
        {
            fadePanel.rectTransform.anchoredPosition = startPosition;
            fadePanel.color = new Color(0f, 0f, 0f, 1f); // Musta, t‰ysin n‰kyv‰
        }
    }

    void Update()
    {
        // Ei tarvetta Update-metodille t‰ss‰ tapauksessa
    }

    public void StartGame()
    {
        // K‰ynnist‰ feidaus ennen scenen vaihtoa
        StartCoroutine(FadeAndLoadScene("DemoTwon"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadePanel != null)
        {
            // Liikuta paneelia vasemmalta oikealle
            float elapsedTime = 0f;
            RectTransform panelTransform = fadePanel.rectTransform;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime);
                yield return null; // Odota seuraava frame
            }

            // Varmista, ett‰ paneeli on lopullisessa positiossa
            panelTransform.anchoredPosition = endPosition;
        }

        // Vaihda scene feidauksen j‰lkeen
       SceneManager.LoadScene(sceneName);
    }
}