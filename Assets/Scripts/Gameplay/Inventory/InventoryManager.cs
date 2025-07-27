using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InventoryManager : NetworkBehaviour
{
    [SyncVar] public bool isInventoryOpen;
    public List<Item> inventorySlots = new List<Item>(32);
    public Transform inventoryCanvas;
    public GameObject slotPrefab;       // InventorySlot prefab
    public Transform slotParent;        // A UI container in canvas


    public void AddItem(Item item)
    {
        inventorySlots.Add(item);
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        // Clear existing slots
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // Create new slots based on inventory
        foreach (Item item in inventorySlots)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.SetItem(item); // Assuming your slot prefab has this method
            }
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.I)) // Open
        {
            isInventoryOpen = true;
            CmdSetInventoryState(isInventoryOpen);
            RpcUpdateCanvasState(isInventoryOpen);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // Close
        {
            isInventoryOpen = false;
            CmdSetInventoryState(isInventoryOpen);
            RpcUpdateCanvasState(isInventoryOpen);
        }

        if (Input.GetKeyDown(KeyCode.P)) // Close
        {
            ClearInventory();
        }
    }
    public void ClearInventory()
    {
        inventorySlots.Clear();
        UpdateInventoryUI();
        Debug.Log("Inventory cleared!");
    }

    [Command]
    void CmdSetInventoryState(bool open)
    {
        isInventoryOpen = open;
    }

    [ClientRpc]
    void RpcUpdateCanvasState(bool open)
    {
        inventoryCanvas.gameObject.SetActive(open);
    }
}
