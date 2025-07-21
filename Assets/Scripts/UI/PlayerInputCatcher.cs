using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace SteamLobby
{
	public class PlayerInputCatcher : NetworkBehaviour
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

            // Make sure IngameUI is available
            if (IngameUI.Instance == null) return;
            // Check if escape is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Opened the escape menu!");
                IngameUI.Instance.OpenEscape();
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