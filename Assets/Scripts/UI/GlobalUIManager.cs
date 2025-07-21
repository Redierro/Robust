using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class GlobalUIManager : NetworkBehaviour
    {
        public static GlobalUIManager Instance;

        public GameObject OnDisconnectedPanel;

        private void Awake()
        {
            Instance = this;
        }
    }
}
