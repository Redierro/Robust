using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class PlayerNetworking : NetworkBehaviour
    {
        [Command]
        public void CmdDropItem(string itemName, Vector3 position)
        {
            Debug.Log($"[CmdDropItem] Server received drop request for: {itemName}");

            Item itemToDrop = ItemManager.Instance.GetItemByName(itemName);
            if (itemToDrop?.prefab != null)
            {
                GameObject droppedItem = Instantiate(itemToDrop.prefab, position, Quaternion.identity);
                NetworkServer.Spawn(droppedItem);
            }
        }
    }
}