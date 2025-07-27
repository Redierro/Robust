using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour
{
    public Item itemData;
    public void CmdPickupItem(GameObject player)
    {
        if (player == null) { Debug.LogError("Couldn't find player from connection"); return; }
        player.GetComponentInChildren<InventoryManager>().AddItem(itemData);
        Debug.Log("Added " + itemData.name + " to the inventory!");
        NetworkServer.Destroy(gameObject);
    }
}
