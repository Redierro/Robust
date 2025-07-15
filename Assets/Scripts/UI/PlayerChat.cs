using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SteamLobby
{
	public class PlayerChat : NetworkBehaviour
	{
        void Update()
        {
            // Only the local player should detect input
            if (!isLocalPlayer) return;

            // Make sure ChatManager is available
            if (ChatManager.Instance == null) return;

            // Check if chat field is focused and Enter is pressed
            if (ChatManager.Instance.chatField.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                ChatManager.Instance.SendMessage();
            }
        }
        [Command]
        public void CmdSendMessage(string message)
        {
            RpcReceiveMessage(message);
        }

        [ClientRpc]
        void RpcReceiveMessage(string message)
        {
            ChatManager.Instance.ReceiveMessage(message);
        }
    }
}