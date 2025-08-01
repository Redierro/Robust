using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item itemData;
    public Transform originalSlot;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI label;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(Item item)
    {
        itemData = item;

        if (icon != null && item.icon != null)
            icon.sprite = item.icon;

        if (label != null)
            label.text = item.itemName;

        Debug.Log("ItemUI setup for: " + item.itemName);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[Drag Start] Dragging item: {itemData.name} from slot {transform.parent.name}");

        originalSlot = transform.parent;

        transform.SetParent(transform.root, true);          // keep world position
        transform.SetAsLastSibling();                       // render above everything

        RectTransform rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f);                 // center it
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        canvasGroup.blocksRaycasts = false;
    }


    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        InventorySlotUI newSlot = eventData.pointerEnter?.GetComponentInParent<InventorySlotUI>();
        Debug.Log($"[Drag End] Dropped item: {itemData.name} in slot {newSlot}");

        if (newSlot != null)
        {
            InventorySlotUI sourceSlot = originalSlot.GetComponent<InventorySlotUI>();
            newSlot.SetItem(this, sourceSlot); // <-- Pass the sourceSlot directly

            if (originalSlot != newSlot)
            {
                sourceSlot.currentItem = null;
                // Item will already be re-parented
            }
        }
        else
        {
            transform.SetParent(originalSlot, false);
            ResetTransform();
        }
    }

    private void ResetTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
