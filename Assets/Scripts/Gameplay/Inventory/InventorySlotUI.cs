using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text itemName;

    public void SetItem(Item item)
    {
        if (itemIcon != null)
            itemIcon.sprite = item.icon;

        if (itemName != null)
            itemName.text = item.itemName;
    }
}
