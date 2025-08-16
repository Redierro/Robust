using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerNetworkBridge : NetworkBehaviour
{
    [Command]
    public void CmdDropItem(string itemName, Vector3 position)
    {
        Debug.Log($"[Server] CmdDropItem received for: {itemName}");

        Item itemToDrop = ItemManager.Instance.GetItemByName(itemName);
        if (itemToDrop?.prefab != null)
        {
            GameObject droppedItem = Instantiate(itemToDrop.prefab, position, Quaternion.identity);
            NetworkServer.Spawn(droppedItem);
        }
    }
}
