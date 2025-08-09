using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Simple UI slot; quick slots are limited and known ahead of time.
public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    public Image backgroundImage;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private InventoryManager manager;
    private int index;

    public void Initialize(InventoryManager manager, int index)
    {
        this.manager = manager;
        this.index = index;
        Deselect();
    }

    public void Select()
    {
        if (backgroundImage != null) backgroundImage.color = selectedColor;
    }

    public void Deselect()
    {
        if (backgroundImage != null) backgroundImage.color = normalColor;
    }

    // Called by InventoryManager when a slot's stack changes
    public void RefreshFromStack(object stackLike)
    {
        // Placeholder for when InventoryManager has reference to its UI prefab instances.
        // In this design InventoryManager calls QuickSlotUI.RefreshFromStack with a private ItemStack.
    }

    public void Clear()
    {
        // Clear visuals: you can implement visual clearing here (e.g. destroy child InventoryItemUI).
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dropped = eventData.pointerDrag;
        if (dropped == null) return;
        var itemUi = dropped.GetComponent<InventoryItemUI>();
        if (itemUi == null) return;

        itemUi.transform.SetParent(transform, false);
    }
}
