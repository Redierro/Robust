using UnityEngine;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;
using UnityEngine.EventSystems;

namespace SteamLobby
{
    public class ChatManager : NetworkBehaviour
    {
        public static ChatManager Instance;

        public TMP_InputField chatField;
        public TMP_Text chatMessages;
        private int chatLeftLenght = 160;
        public TMP_Text chatLeftLenghtText;
        public GameObject upperPanel;
        private bool upperPanelRaised = false;

        private void Awake()
        {
            Instance = this;
        }
        private void Update()
        {
            chatLeftLenght = 160 - chatField.text.Trim().Length;
            chatLeftLenghtText.text = chatLeftLenght.ToString();
            ChatVisibility(upperPanelRaised);
        }
        public void SendMessage()
        {
            string message = chatField.text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log("Sent message: " + message);
                var player = NetworkClient.localPlayer;
                if (player != null)
                {
                    player.GetComponent<PlayerChat>().CmdSendMessage($"<color=#3FA7F2>{SteamFriends.GetPersonaName()}</color>: " + message);
                }
                chatField.text = "";

                // Refocus input field on the next frame
                StartCoroutine(RefocusInputField());
            }
        }
        private void ChatVisibility(bool isRaised)
        {
            if (Input.GetKeyDown(KeyCode.Return) && !isRaised)
            {
                upperPanel.SetActive(true);
                upperPanelRaised = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && isRaised)
            {
                upperPanel.SetActive(false);
                upperPanelRaised = false;
            }
        }

        public void ReceiveMessage(string message)
        {
            chatMessages.text += message + "\n";
        }
        private IEnumerator RefocusInputField()
        {
            yield return null; // Wait one frame
            chatField.ActivateInputField();
        }
        [Server]
        public void BroadcastServerMessage(string message)
        {
            RpcReceiveServerMessage(message);
        }

        [ClientRpc]
        void RpcReceiveServerMessage(string formattedMsg)
        {
            ReceiveMessage(formattedMsg);
        }
    }
}
