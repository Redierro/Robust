using UnityEngine;
using Mirror;
public class SlotUI : MonoBehaviour
{
    public ItemUI currentItem;

    public bool IsEmpty() => currentItem == null;

    public void SetItem(ItemUI item)
    {
        currentItem = item;
        item.transform.SetParent(transform, false);
    }

    public void ClearItem()
    {
        currentItem = null;
    }
}

