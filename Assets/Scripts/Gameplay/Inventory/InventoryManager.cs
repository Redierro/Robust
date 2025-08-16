using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

namespace SteamLobby
{
    public class InventoryManager : NetworkBehaviour
    {
        public RectTransform inventoryCanvas;
        public List<InventorySlotUI> slots; // Assigned in Inspector not grabbed during runtime
        public GameObject itemUIPrefab;
        public Transform playerObject;
        public RectTransform inventoryBounds;

        // Local checking
        public bool isInventoryOpen => inventoryCanvas != null && inventoryCanvas.gameObject.activeSelf;
        // Network checking
        [SyncVar(hook = nameof(OnInventoryStateChanged))]
        private bool isInventoryOpenNetworked;

        [SerializeField] private CameraTransitionController camController;
        public PlayerController playerController;

        public void TryAddItem(Item item)
        {
            foreach (InventorySlotUI slot in slots)
            {
                if (slot.IsEmpty())
                {
                    GameObject itemGO = Instantiate(itemUIPrefab, slot.transform, false);
                    itemGO.transform.localPosition = Vector3.zero;
                    itemGO.transform.localScale = Vector3.one;

                    ItemUI itemUI = itemGO.GetComponent<ItemUI>();
                    itemUI.Setup(item);
                    slot.SetItem(itemUI, null, true);
                    return;
                }
            }

            Debug.Log("Inventory is full!");
        }

        public void OpenInventory()
        {
            inventoryCanvas.gameObject.SetActive(true);
            camController?.TransitionToInventory();
            playerController.isMovementLocked = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseInventory()
        {
            inventoryCanvas.gameObject.SetActive(false);
            camController?.TransitionToDefault();
            playerController.isMovementLocked = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void DropItem(Item itemData)
        {
            if (itemData == null)
                return;

            Vector3 dropPosition = playerObject.position + playerObject.forward * 2f;

            if (isServer)
            {
                // Host/server handles it directly
                ServerDropItem(itemData.itemName, dropPosition);
            }
            else
            {
                // Client asks server to spawn
                CmdDropItem(itemData.itemName, dropPosition);
            }
        }

        public void ClearInventory()
        {
            foreach (var slot in slots)
                slot.ClearItem();
        }

        void OnInventoryStateChanged(bool oldValue, bool newValue)
        {
            inventoryCanvas.gameObject.SetActive(newValue);
            // Apply camera, cursor, movement lock here
        }

        [Command]
        public void CmdSetInventoryState(bool isOpen)
        {
            isInventoryOpenNetworked = isOpen;
        }

        [Command]
        private void CmdDropItem(string itemName, Vector3 position)
        {
            // For clients -> delegate to server logic
            ServerDropItem(itemName, position);
        }

        [Server]
        private void ServerDropItem(string itemName, Vector3 position)
        {
            Debug.Log($"[Server] DropItem executed for: {itemName}");

            Item itemToDrop = ItemManager.Instance.GetItemByName(itemName);
            if (itemToDrop?.prefab != null)
            {
                GameObject droppedItem = Instantiate(itemToDrop.prefab, position, Quaternion.identity);
                NetworkServer.Spawn(droppedItem);
            }
        }
    }
}