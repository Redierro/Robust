using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Steamworks;

namespace SteamLobby
{
    public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Item itemData;
        public Transform originalSlot;

        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        private InventoryManager inventoryManager;

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

            transform.SetParent(originalSlot.parent, true);          // keep world position
            transform.SetAsLastSibling();                       // render above everything

            RectTransform rt = GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);                 // center it
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Track the item's position to show it being dragged
            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint
            );

            transform.position = worldPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();

            if (IsOutsideInventory(eventData, inventoryManager.inventoryBounds) && inventoryManager.isLocalPlayer)
            {
                inventoryManager.DropItem(itemData);
                Destroy(gameObject); // Remove the item from the UI
                return;
            }

            InventorySlotUI newSlot = eventData.pointerEnter?.GetComponentInParent<InventorySlotUI>();
            Debug.Log($"[Drag End] Dropped item: {itemData.name} in slot {newSlot}");

            if (newSlot != null)
            {
                InventorySlotUI sourceSlot = originalSlot.GetComponent<InventorySlotUI>();
                newSlot.SetItem(this, sourceSlot, false);
            }
            else
            {
                transform.SetParent(originalSlot, false);
                ResetTransform();
            }
        }
        private bool IsOutsideInventory(PointerEventData eventData, RectTransform inventoryBounds)
        {
            return !RectTransformUtility.RectangleContainsScreenPoint(
                inventoryBounds,
                eventData.position,
                eventData.pressEventCamera
            );
        }

        private void ResetTransform()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}

