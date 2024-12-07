using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;

    private DialogueUI dialogueUI;
    private ResponseEvent[] responseEvents;

    List<GameObject> tempResponseButtons = new List<GameObject>();

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }

    public void ShowResponses(Response[] responses)
    {
        if (responseBox == null)
        {
            Debug.LogError("ResponseBox is not assigned in the Inspector.");
            return;
        }

        if (responses == null || responses.Length == 0)
        {
            Debug.LogError("Responses are null or empty.");
            return;
        }

        float responseBoxHeight = 0;

        for (int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            if (response == null || string.IsNullOrEmpty(response.ResponseText))
            {
                Debug.LogWarning("Empty or null response detected. Skipping.");
                continue;
            }

            int localIndex = i; // Paikallinen kopio indeksistä
            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            responseButton.gameObject.SetActive(true);
            responseButton.GetComponentInChildren<TMP_Text>().text = response.ResponseText;
            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response, localIndex));

            tempResponseButtons.Add(responseButton);
            responseBoxHeight += responseButtonTemplate.sizeDelta.y;
        }

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
        responseBox.gameObject.SetActive(true);
    }

    private void OnPickedResponse(Response response, int responseIndex)
    {
        responseBox.gameObject.SetActive(false);

        foreach (GameObject responseButton in tempResponseButtons)
        {
            Destroy(responseButton);
        }

        if (responseEvents != null && responseEvents.Length > 0 && responseIndex < responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();
        }

        responseEvents = null;

        if (response.DialogueObject != null)
        {
            dialogueUI.ShowDialogue(response.DialogueObject); // Käytä olemassa olevaa instanssia
        }
        else
        {
            dialogueUI.CloseDialogueBox();
        }
    }
}
