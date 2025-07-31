using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    public ItemUI currentItem;

    public bool IsEmpty() => currentItem == null;

    public void SetItem(ItemUI item)
    {
        if (item == null)
        {
            currentItem = null;
            return;
        }

        if (!IsEmpty())
        {
            ItemUI temp = currentItem;
            if (item.originalSlot != null)
            {
                currentItem.transform.SetParent(item.originalSlot, false);
                item.originalSlot.GetComponent<InventorySlotUI>().SetItem(temp);
            }
            else
            {
                Debug.LogWarning($"[SetItem] Swap aborted: originalSlot was null for {item.name}");
            }
        }

        currentItem = item;
        item.transform.SetParent(transform, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
    }

    public void ClearItem(bool destroyObject = true)
    {
        if (destroyObject && currentItem != null)
            Destroy(currentItem.gameObject);

        currentItem = null;
    }
}
