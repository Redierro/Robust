using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    public ItemUI currentItem;
    public ItemUI previousItem;

    public bool IsEmpty() => currentItem == null;

    public void SetItem(ItemUI item, InventorySlotUI sourceSlot)
    {
        if (item == null)
        {
            Debug.LogError("Something went wrong. Item was not added.");
            currentItem = null;
            return;
        }

        if (this == sourceSlot)
        {
            // Dropped onto own slot
            item.transform.SetParent(transform, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            currentItem = item;

            Debug.Log($"[SetItem] {item.name} returned to slot {name}");
            return;
        }
        if (!IsEmpty() && currentItem != item && sourceSlot != null)
        {
            // Swap: put current item into source slot
            previousItem = currentItem;

            /// Source slot = the onenddrag slot

            if (sourceSlot != null)
            {
                Debug.Log("! - A swap happened - !");
                // Assign the old item to the old slot
                sourceSlot.currentItem = previousItem;
                previousItem.transform.SetParent(sourceSlot.transform, false);
                previousItem.transform.localPosition = Vector3.zero;
                previousItem.transform.localRotation = Quaternion.identity;
                previousItem.transform.localScale = Vector3.one;
            }
            else
            {
                Destroy(previousItem.gameObject);
            }
        }
        else { previousItem = null; }

        // Place new item into this slot
        currentItem = item;
        item.transform.SetParent(transform, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        Debug.Log($"[SetItem] Placed {item.name} into slot {name}");
    }


    public void ClearItem(bool destroyObject = true)
    {
        if (destroyObject && currentItem != null)
            Destroy(currentItem.gameObject);

        currentItem = null;
    }
}
