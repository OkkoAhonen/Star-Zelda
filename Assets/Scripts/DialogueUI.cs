using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textlabel;
    [SerializeField] private DialogueObject testDialogue;

    private TypeWriterEffect typeWriterEffect;

    private void Awake()
    {
        if (textlabel == null)
        {
            textlabel = GetComponentInChildren<TMP_Text>();
        }

        typeWriterEffect = GetComponent<TypeWriterEffect>(); // Alustetaan heti
        CloseDialogueBox();
        if (testDialogue != null)
        {
            ShowDialogue(testDialogue);
        }
        else
        {
            Debug.LogWarning("DialogueObject is not assigned in the Inspector.");
        }
    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        dialogueBox.SetActive(true);
        StartCoroutine(StepTroughDialogue(dialogueObject));
    }

    private IEnumerator StepTroughDialogue(DialogueObject dialogueObject)
    {
        yield return new WaitForSeconds(1f);

        foreach (string dialogue in dialogueObject.Dialogue)
        {
            yield return typeWriterEffect.Run(dialogue, textlabel);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        CloseDialogueBox();
    }

    private void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        textlabel.text = string.Empty;
    }
}
