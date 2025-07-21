using UnityEngine;
using System.Collections.Generic;
using Mirror;
using System.Collections;
using Steamworks;
using UnityEngine.SceneManagement;

namespace SteamLobby
{
    public class SteamLobbySC : NetworkBehaviour
    {
        public static SteamLobbySC Instance;
        [SerializeField] private ChatManager chatManager;
        [SerializeField] private GameObject hostButton = null;
        public ulong lobbyID;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private PanelSwapper panelSwapper;
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

        private const string HostAddressKey = "HostAddress";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            networkManager = NetworkManager.singleton;
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized");
                return;
            }
            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                chatManager = ChatManager.Instance;
                panelSwapper = GameObject.Find("PanelSwapper").GetComponent<PanelSwapper>();
                hostButton = GameObject.Find("HostLobbyButton");
            }

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }
        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
            ChatManager.Instance.enabled = true;
        }
        public void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Failed to create lobby: " + callback.m_eResult);
                return;
            }

            Debug.Log("Lobby sucesfully created. Lobby ID: " + callback.m_ulSteamIDLobby);
            networkManager.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            lobbyID = callback.m_ulSteamIDLobby;
        }
        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Join request received for lobby: " + callback.m_steamIDLobby);

            if (NetworkClient.isConnected || NetworkClient.active)
            {
                Debug.Log("NetworkClient is active or connected. Disconnecting before joining new lobby.");
                NetworkManager.singleton.StopClient();
                NetworkClient.Shutdown();
            }
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }
        void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Already in a lobby as a host. Ignoring request.");
                return;
            }
            lobbyID = callback.m_ulSteamIDLobby;
            string _hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            networkManager.networkAddress = _hostAddress;
            Debug.Log("Entered lobby: " + callback.m_ulSteamIDLobby);
            networkManager.StartClient();
            panelSwapper.SwapPanel("LobbyPanel");
        }
        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby != lobbyID) return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            Debug.Log($"LobbyChatUpdate: {stateChange}");

            bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);
            if (shouldUpdate)
            {
                StartCoroutine(DelayedNameUpdate(0.5f));
                LobbyUIManager.Instance?.CheckAllPlayersReady();
            }
            ServerMessage(stateChange, callback);
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance == null)
            {
                Debug.LogWarning("Lobby UI Manager. Instance is null, skipping name update");
                yield break;
            }
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI();
        }

        public void LeaveLobby()
        {
            if (NetworkServer.active && SceneManager.GetActiveScene().name == "GameplayScene")
            {
                CustomNetworkManager.singleton.ServerChangeScene("SampleScene");
            }
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(lobbyID);
            List<CSteamID> members = new List<CSteamID>();

            int count = SteamMatchmaking.GetNumLobbyMembers(lobby);

            for (int i = 0; i < count; i++)
            {
                members.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            }

            if (lobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
                lobbyID = 0;
            }

            if (NetworkServer.active && currentOwner == me) // IF host, stop hosting
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected) // IF client, stop connection
            {
                NetworkManager.singleton.StopClient(); 
            }

            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                panelSwapper.gameObject.SetActive(true);
                this.gameObject.SetActive(true);
                panelSwapper.SwapPanel("MainPanel");

                ChatManager.Instance.enabled = false; // So theres no opening chat while not in lobby
            }
            chatManager.chatMessages.text = ""; // Clear text when joining the lobby so it doesnt transfer from another lobby 
            Debug.Log("Clearing chat...");
        }

        public void ServerMessage(EChatMemberStateChange stateChange, LobbyChatUpdate_t callback)
        {
            // Chat message that someone left
            CSteamID changedMember = (CSteamID)callback.m_ulSteamIDUserChanged;
            string playerName = SteamFriends.GetFriendPersonaName(changedMember);

            if (stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked))
            {
                Debug.Log(playerName + " has left the lobby.");

                ChatManager.Instance?.ReceiveMessage($"{playerName} <color=#8B0000>has left the lobby.</color>");
            }

            // Chat message that someone entered
            if (stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
            {
                Debug.Log(playerName + " has joined the lobby.");

                ChatManager.Instance?.ReceiveMessage($"{playerName} <color=#006400>has joined the lobby.</color>");
            }
        }
    }
}
