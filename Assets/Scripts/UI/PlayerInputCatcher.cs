using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace SteamLobby
{
	public class PlayerInputCatcher : NetworkBehaviour
	{
        [SerializeField] private IngameUI igUI;
        [SerializeField] private InventoryManager invManage;
        private bool escapeHandledThisFrame = false;
        void Update()
        {
            if (!isLocalPlayer) return;
            if (ChatManager.Instance == null) return;

            if (escapeHandledThisFrame)
            {
                escapeHandledThisFrame = false;
                return;
            }

            ///
            /// FROM HERE ON OUT SAMPLESCENE AND GAMEPLAY
            /// 

            // Handle Enter (chat)
            if (Input.GetKeyDown(KeyCode.Return) && !ChatManager.Instance.chatRaised)
            {
                Debug.Log("Sent a message!");
                ChatManager.Instance.OpenChat();
            }
            else if (Input.GetKeyDown(KeyCode.Return) && ChatManager.Instance.chatRaised)
            {
                ChatManager.Instance.SendMessage();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "SampleScene")
            {
                if (ChatManager.Instance.chatRaised)
                {
                    ChatManager.Instance.CloseChat();
                }
            }

            ///
            /// FROM HERE ON OUT ONLY GAMEPLAY
            /// 

            if (SceneManager.GetActiveScene().name == "GameplayScene")
            {
                // Handle I (Inventory)
                if (Input.GetKeyDown(KeyCode.I)) // Open inv
                {
                    invManage.OpenInventory();
                }
                if (Input.GetKeyDown(KeyCode.P)) // Clear inv
                {
                    invManage.ClearInventory();
                }
                // Handle Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (invManage.isInventoryOpen)
                    {
                        invManage.CloseInventory(); // First priority
                    }
                    else if (ChatManager.Instance.chatRaised)
                    {
                        ChatManager.Instance.CloseChat();
                    }
                    else
                    {
                        Debug.Log("Opened the escape menu!");
                        igUI.OpenEscape(); // Only opens if no other UI is raised
                    }

                    escapeHandledThisFrame = true; // Block further Escape handling this frame
                }
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