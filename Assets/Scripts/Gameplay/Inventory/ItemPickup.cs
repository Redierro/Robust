using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour, IInteractable
{
    [SerializeField] private Item itemData; // Assign in inspector

    public string GetInteractionText()
        => $"Press [E] to pick up {itemData.itemName}";

    public void Interact(GameObject player)
        => CmdPickupItem(player);

    [ClientRpc]
    void CmdPickupItem(GameObject player)
    {
        player.GetComponentInChildren<InventoryManager>().AddItem(itemData);
        NetworkServer.Destroy(gameObject);
    }
}
