using UnityEngine;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;

namespace SteamLobby
{
    public class LobbyUIManager : NetworkBehaviour
    {
        public static LobbyUIManager Instance;
        public Transform playerListParent;
        public List<TextMeshProUGUI> playerNameTexts = new List<TextMeshProUGUI>();
        public List<PlayerLobbyHandler> playerLobbyHandlers = new List<PlayerLobbyHandler>();
        public Button playGameButton;

        private int lastReadyCount = -1; // To avoid doubling of messages

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            playGameButton.interactable = false;
        }

        public void UpdatePlayerLobbyUI()
        {
            playerNameTexts.Clear();
            playerLobbyHandlers.Clear();

            var lobby = new CSteamID(SteamLobbySC.Instance.lobbyID);
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);

            CSteamID hostID = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(lobby, "HostAddress")));
            List<CSteamID> orderedMembers = new List<CSteamID>();

            if (memberCount == 0)
            {
                Debug.LogWarning("Lobby has no members.. retrying...");
                StartCoroutine(RetryUpdate());
                return;
            }

            orderedMembers.Add(hostID);

            for (int i = 0; i < memberCount; i++)
            {
                CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
                if (memberID != hostID)
                {
                    orderedMembers.Add(memberID);
                }
            }

            int j = 0;

            foreach (var member in orderedMembers)
            {
                TextMeshProUGUI txtMesh = playerListParent.GetChild(j).GetChild(0).GetComponent<TextMeshProUGUI>();
                PlayerLobbyHandler playerLobbyHandler = playerListParent.GetChild(j).GetComponent<PlayerLobbyHandler>();

                playerLobbyHandlers.Add(playerLobbyHandler);
                playerNameTexts.Add(txtMesh);

                string playerName = SteamFriends.GetFriendPersonaName(member);
                playerNameTexts[j].text = playerName;
                j++;
            }
            CheckAllPlayersReady();
        }

        public void OnPlayButtonClicked()
        {
            if (NetworkServer.active)
            {
                CustomNetworkManager.singleton.ServerChangeScene("GameplayScene");
            }
        }
        public void RegisterPlayer(PlayerLobbyHandler player)
        {
            player.transform.SetParent(playerListParent, false);
            UpdatePlayerLobbyUI();
        }
        [Server]
        public void CheckAllPlayersReady()
        {
            int readyCount = 0;
            int totalCount = playerLobbyHandlers.Count;

            foreach (var player in playerLobbyHandlers)
            {
                if (player.isReady)
                    readyCount++;
            }

            RpcSetPlayButtonInteractable(readyCount == totalCount);

            // Only send chat message when all are ready and status changed
            if (readyCount == totalCount && readyCount != lastReadyCount)
            {
                ChatManager.Instance?.BroadcastServerMessage("<color=#006400>All players are ready. You may start the game.</color>");
            }

            lastReadyCount = readyCount;
        }
        [ClientRpc]
        void RpcSetPlayButtonInteractable(bool truthStatus)
        {
            playGameButton.interactable = truthStatus;
        }
        private IEnumerator RetryUpdate()
        {
            yield return new WaitForSeconds(1f);
            UpdatePlayerLobbyUI();
        }
    }
}
