using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour, IInteractable
{
    [SerializeField] private Item itemData;

    public string GetInteractionText()
        => $"Press [E] to pick up {itemData.itemName}";

    public void Interact(GameObject player)
        => CmdPickupItem();

    [Command]
    void CmdPickupItem()
    {
        var playerIdentity = connectionToClient.identity;
        if (playerIdentity == null)
        {
            Debug.LogWarning("No player identity found!");
            return;
        }

        var inventory = playerIdentity.GetComponentInChildren<InventoryManager>();
        if (inventory != null)
        {
            inventory.AddItem(itemData);
            NetworkServer.Destroy(gameObject);
        }
    }
}
