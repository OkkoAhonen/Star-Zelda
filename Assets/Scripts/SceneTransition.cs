using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image fadePanel;
    public float fadeSpeed = 2f;

    private Vector2 startPosition = new Vector2(0f, 0f); // Keskeltä
    private Vector2 endPosition = new Vector2(3840f, 0f); // Oikealle ulkopuolelle (tuplattu varmuuden vuoksi)

    void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.rectTransform.sizeDelta = new Vector2(Screen.width * 2, Screen.height); // Varmista koko
            fadePanel.rectTransform.anchoredPosition = startPosition;
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            StartCoroutine(FadeOut());
        }
    }
    private void Update()
    {

    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        RectTransform panelTransform = fadePanel.rectTransform;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime);
            yield return null;
        }
        panelTransform.anchoredPosition = endPosition;
        fadePanel.gameObject.SetActive(false);
    }
}