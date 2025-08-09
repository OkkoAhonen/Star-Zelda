using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Visual representation of an item in a slot. This is strictly UI.
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Text countText;

    private RectTransform rect;
    private Canvas rootCanvas;
    private Transform parentAfterDrag;

    private string itemID;
    private int count;

    public void Initialize(Item item, int stackCount)
    {
        itemID = item != null ? item.ID : "";
        count = stackCount;
        image.sprite = item != null ? item.Image : null;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count > 1 ? count.ToString() : "";
    }

    public void SetCount(int c)
    {
        count = c;
        RefreshCount();
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag, true);
        image.raycastTarget = true;
    }
}
