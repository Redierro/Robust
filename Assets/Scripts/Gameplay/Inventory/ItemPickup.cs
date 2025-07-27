using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour, IInteractable
{
    [SerializeField] private Item itemData; // Assign in inspector

    public string GetInteractionText()
        => $"Press [E] to pick up {itemData.itemName}";

    public void Interact(GameObject player)
    {
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        CmdPickupItem(identity);
    }

    [Command]
    void CmdPickupItem(NetworkIdentity playerId)
    {
        InventoryManager inventory = playerId.GetComponentInChildren<InventoryManager>();
        inventory.AddItem(itemData);
        NetworkServer.Destroy(gameObject);
    }
}
