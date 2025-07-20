using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

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
            // Check if Enter is pressed
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Sent a message!");
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