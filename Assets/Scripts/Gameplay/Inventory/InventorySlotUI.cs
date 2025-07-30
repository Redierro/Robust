using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public ItemUI currentItemUI;

    public bool IsEmpty() => currentItemUI == null;

    public Image itemIcon;
    public TMP_Text itemName;

    public void SetItem(Item item, GameObject itemUIPrefab)
    {
        if (!IsEmpty()) return;

        GameObject newItem = Instantiate(itemUIPrefab, transform);
        currentItemUI = newItem.GetComponent<ItemUI>();
        currentItemUI.Setup(item);
    }
    public void ClearItem()
    {
        if (currentItemUI != null)
            Destroy(currentItemUI.gameObject);

        currentItemUI = null;
    }
    public void AssignItemUI(ItemUI ui)
    {
        currentItemUI = ui;
        ui.transform.SetParent(transform, false);
    }
