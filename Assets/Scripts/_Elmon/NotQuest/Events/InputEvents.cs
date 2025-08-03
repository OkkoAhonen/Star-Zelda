using UnityEngine;
using System;

public class InputEvents
{
    public InputEventContext inputEventContext { get; private set; } = InputEventContext.DEFAULT;

    public void ChangeInputEventContext(InputEventContext newContext)
    {
        this.inputEventContext = newContext;
    }

    public event Action<Vector2> onMovePressed;
    public void MovePressed(Vector2 moveDir)
    {
        if (onMovePressed != null)
        {
            onMovePressed(moveDir);
        }
    }

    public event Action<InputEventContext> OnSubmitPressed;
    public void SubmitPressed()
    {
        if (OnSubmitPressed != null)
        {
            OnSubmitPressed(this.inputEventContext);
        }
    }

    public event Action onQuestLogTogglePressed;
    public void QuestLogTogglePressed()
    {
        if (onQuestLogTogglePressed != null)
        {
            onQuestLogTogglePressed();
        }
    }

    public event Action onInventory;
    public void InventoryPressed()
    {
        onInventory?.Invoke();
    }

    public event Action onBookToggle;
    public void BookToggled()
    {
        onBookToggle?.Invoke();
    }

    public event Action onLastScene;
    public void LastScene()
    {
        onLastScene?.Invoke();
    }

    public event Action onNextScene;
    public void NextScene()
    {
        onNextScene?.Invoke();
    }

    public event Action onHealPressed;
    public void HealPressed()
    {
        onHealPressed?.Invoke();
    }
}