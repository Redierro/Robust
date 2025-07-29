using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class InventoryManager : NetworkBehaviour
    {
        [SyncVar] public bool isInventoryOpen;
        public List<Item> inventorySlots = new List<Item>(32);
        public Transform inventoryCanvas;
        public GameObject slotPrefab;       // InventorySlot prefab
        public Transform slotParent;        // A UI container in canvas
        [SerializeField] private CameraTransitionController camController;
        private PlayerController playerController;

        void Start()
        {
            playerController = GetComponentInParent<PlayerController>();
        }
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

        public void OpenInventory()
        {
            isInventoryOpen = true;
            CmdSetInventoryState(isInventoryOpen);

            camController?.TransitionToInventory();
            playerController.isMovementLocked = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void CloseInventory()
        {
            isInventoryOpen = false;
            CmdSetInventoryState(isInventoryOpen);

            camController?.TransitionToDefault();
            playerController.isMovementLocked = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            RpcUpdateCanvasState(open);
        }

        [ClientRpc]
        void RpcUpdateCanvasState(bool open)
        {
            inventoryCanvas.gameObject.SetActive(open);
        }
    }

}