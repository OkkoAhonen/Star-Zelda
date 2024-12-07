using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textlabel;
    //[SerializeField] private DialogueObject testDialogue;

    private ResponseHandler responseHandler;
    private TypeWriterEffect typeWriterEffect;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        // Tarkistetaan, että kaikki komponentit ovat olemassa
        if (textlabel == null)
        {
            textlabel = GetComponentInChildren<TMP_Text>();
            if (textlabel == null)
            {
                Debug.LogError("TextLabel is not assigned, and no TMP_Text component was found in children.");
            }
        }

        typeWriterEffect = GetComponent<TypeWriterEffect>();
        if (typeWriterEffect == null)
        {
            Debug.LogError("TypeWriterEffect component is missing.");
        }

        responseHandler = GetComponent<ResponseHandler>();
        if (responseHandler == null)
        {
            Debug.LogError("ResponseHandler component is missing.");
        }

        if (dialogueBox == null)
        {
            Debug.LogError("DialogueBox is not assigned in the Inspector.");
        }

        CloseDialogueBox();

    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        IsOpen = true;
        if (dialogueObject == null)
        {
            Debug.LogError("DialogueObject is null. Cannot start dialogue.");
            return;
        }

        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObject));
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        responseHandler.AddResponseEvents(responseEvents);
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        if (dialogueObject.Dialogue == null || dialogueObject.Dialogue.Length == 0)
        {
            Debug.LogError("DialogueObject.Dialogue is null or empty.");
            yield break;
        }

        for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i];

            yield return RuntypingEffect(dialogue);

            textlabel.text = dialogue;  

            yield return null;

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses) break;


        }

        if (dialogueObject.HasResponses)
        {
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            CloseDialogueBox();
        }
    }

    private IEnumerator RuntypingEffect(string dialogue)
    {
        typeWriterEffect.Run(dialogue, textlabel);

        while (typeWriterEffect.IsRunning)
        {
            yield return null;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                typeWriterEffect.Stop();
            }
        }
    }

    public void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueBox.SetActive(false);
        if (textlabel != null)
        {
            textlabel.text = string.Empty;
        }
    }
}
