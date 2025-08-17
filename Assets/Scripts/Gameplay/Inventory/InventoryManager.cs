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
        public static InventoryManager LocalInstance { get; private set; }

        // Local checking
        public bool isInventoryOpen => inventoryCanvas != null && inventoryCanvas.gameObject.activeSelf;
        // Network checking
        [SyncVar(hook = nameof(OnInventoryStateChanged))]
        private bool isInventoryOpenNetworked;

        [SerializeField] private CameraTransitionController camController;
        public PlayerController playerController;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            LocalInstance = this;
        }

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
            if (!isLocalPlayer) return;

            CmdSetInventoryState(true); // Sync to others
            inventoryCanvas.gameObject.SetActive(true);
            camController?.TransitionToInventory();
            playerController.isMovementLocked = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseInventory()
        {
            if (!isLocalPlayer) return;

            CmdSetInventoryState(false); // Sync to others
            inventoryCanvas.gameObject.SetActive(false);
            camController?.TransitionToDefault();
            playerController.isMovementLocked = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void DropItem(Item itemData)
        {
            Debug.Log($"{netId} ({(isLocalPlayer ? "Local" : "Remote")}) dropping item. playerObject = {playerObject.name}");
            if (itemData == null)
                return;

            Vector3 dropPosition = playerObject.position + playerObject.forward * 2f;

            if (isServer)
            {
                // Host executes directly
                ServerDropItem(itemData.itemName, dropPosition);
            }
            else
            {
                // Client asks server
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
            Debug.Log($"[Client] Inventory state changed: {newValue}");
            inventoryCanvas.gameObject.SetActive(newValue);
        }


        [Command]
        public void CmdSetInventoryState(bool isOpen)
        {
            isInventoryOpenNetworked = isOpen;
        }
        [Command]
        private void CmdDropItem(string itemName, Vector3 position)
        {
            ServerDropItem(itemName, position);
        }

        [Server]
        private void ServerDropItem(string itemName, Vector3 position)
        {
            Debug.Log($"[Server] Dropping {itemName} at {position}");

            Item itemToDrop = ItemManager.Instance.GetItemByName(itemName);
            if (itemToDrop?.prefab != null)
            {
                GameObject droppedItem = Instantiate(itemToDrop.prefab, position, Quaternion.identity);
                NetworkServer.Spawn(droppedItem);
            }
        }
    }
}