using UnityEngine;

/*public class DialogueActivator : MonoBehaviour, Interactable
{
    [SerializeField] private DialogueObject dialogueObject;

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      //  if (collision.CompareTag("Player") && collision.TryGetComponent(out PlayerMovement2D player))
        {
            player.Interactable = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
      //  if (collision.CompareTag("Player") && collision.TryGetComponent(out PlayerMovement2D player))
        {
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                player.Interactable = null;
            }
        }
    }

    //public void Interact(PlayerMovement2D player)
    {
        // Tarkistetaan, löytyykö GameObjectista DialogueResponseEvents-komponentteja
        DialogueResponseEvents[] responseEventsArray = GetComponents<DialogueResponseEvents>();

        foreach (DialogueResponseEvents responseEvents in responseEventsArray)
        {
            if (responseEvents.DialogueObject == dialogueObject)
            {
                player.DialogueUI.AddResponseEvents(responseEvents.Events);
            }
        }

        // Näytetään dialogi, jos DialogueObject ei ole tyhjä
        if (dialogueObject != null)
        {
            player.DialogueUI.ShowDialogue(dialogueObject);
        }
    }
}
        `*/