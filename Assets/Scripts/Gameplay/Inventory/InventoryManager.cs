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

            if (isServer && isLocalPlayer)
            {
                // Host dropping directly -> use own connectionToClient
                ServerDropItem(itemData.itemName, connectionToClient);
            }
            else
            {
                // Clients ask the server
                CmdDropItem(itemData.itemName);
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
            // Apply camera, cursor, movement lock here if needed
        }

        [Command]
        public void CmdSetInventoryState(bool isOpen)
        {
            isInventoryOpenNetworked = isOpen;
        }

        [Command]
        private void CmdDropItem(string itemName, NetworkConnectionToClient sender = null)
        {
            // Mirror automatically passes the calling player's connection
            ServerDropItem(itemName, sender);
        }

        [Server]
        private void ServerDropItem(string itemName, NetworkConnectionToClient sender)
        {
            if (sender == null || sender.identity == null)
            {
                Debug.LogWarning("ServerDropItem called with no sender identity!");
                return;
            }

            var inventory = sender.identity.GetComponentInChildren<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("No InventoryManager found on dropping player!");
                return;
            }

            Vector3 dropPosition = inventory.playerObject.position + inventory.playerObject.forward * 2f;
            Debug.Log($"[Server] {sender.identity.netId} dropped {itemName} at {dropPosition}");

            Item itemToDrop = ItemManager.Instance.GetItemByName(itemName);
            if (itemToDrop?.prefab != null)
            {
                GameObject droppedItem = Instantiate(itemToDrop.prefab, dropPosition, Quaternion.identity);
                NetworkServer.Spawn(droppedItem, sender); // sender ensures proper authority if needed
            }
        }
    }
}