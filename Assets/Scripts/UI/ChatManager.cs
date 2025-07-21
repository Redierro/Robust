using UnityEngine;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
        public bool chatRaised = false;
        private bool newMessageReceived = false;

        [SerializeField] private CanvasGroup chatGroup;
        [SerializeField] private float fadeDelay = 5f;
        [SerializeField] private float fadeDuration = 1.5f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            Instance = this;
        }
        private void Update()
        {
            chatLeftLenght = 160 - chatField.text.Trim().Length;
            chatLeftLenghtText.text = chatLeftLenght.ToString();
            ChatVisibility();

            // Trigger fade only when chat is closed and message arrived
            if (!chatRaised && newMessageReceived)
            {
                newMessageReceived = false;

                StartFadeCoroutine();
            }
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
                    player.GetComponent<PlayerInputCatcher>().CmdSendMessage($"<color=#3FA7F2>{SteamFriends.GetPersonaName()}</color>: " + message);
                }
                chatField.text = "";

                // Refocus input field on the next frame
                StartCoroutine(RefocusInputField());
            }
        }
        private void ChatVisibility()
        {
            if (Input.GetKeyDown(KeyCode.Return) && !chatRaised && !IngameUI.Instance.escapeRaised)
            {
                // Cancel fade instantly
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                    fadeCoroutine = null;
                }
                chatGroup.alpha = 1f;

                upperPanel.SetActive(true);
                chatRaised = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                StartCoroutine(RefocusInputField());
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && chatRaised)
            {
                StartFadeCoroutine(); // Left chat, fade the chat away
                upperPanel.SetActive(false);
                chatRaised = false;
                if (!(SceneManager.GetActiveScene().name == "SampleScene"))
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        private void StartFadeCoroutine()
        {
            // Restart fade timer
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            chatGroup.alpha = 1f;
            fadeCoroutine = StartCoroutine(FadeChatAfterDelay());
        }
        public void ReceiveMessage(string message)
        {
            chatMessages.text += message + "\n";
            newMessageReceived = true;
            chatGroup.alpha = 1f; // Restore on sent
        }
        public IEnumerator FadeChatAfterDelay()
        {
            yield return new WaitForSeconds(fadeDelay);

            float elapsed = 0f;
            float startAlpha = chatGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                chatGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                yield return null;
            }

            chatGroup.alpha = 0f;
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
