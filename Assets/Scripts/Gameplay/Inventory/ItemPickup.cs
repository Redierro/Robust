using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class ItemPickup : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Item itemData;
        [SyncVar] public int itemID;
        public string GetInteractionText()
            => $"Press [E] to pick up {itemData.itemName}";

        public void Interact(GameObject player)
        {
            var manager = player.GetComponentInChildren<InventoryManager>();
            if (manager != null)
                manager.TryAddItem(itemData);
        }
    }
}
