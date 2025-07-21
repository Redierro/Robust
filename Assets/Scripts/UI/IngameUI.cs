using UnityEngine;
using Mirror;
using Steamworks;

namespace SteamLobby
{
    public class IngameUI : NetworkBehaviour
    {
        public static IngameUI Instance;
        public GameObject escapeMenu;
        public bool escapeRaised = false;
        private void Awake()
        {
            Instance = this;
        }
        public void OpenEscape()
        {
            if (!escapeRaised && !ChatManager.Instance.chatRaised)
            {
                escapeMenu.SetActive(true);
                escapeRaised = true;
            }
            else if (escapeRaised)
            {
                escapeMenu.SetActive(false);
                escapeRaised = false;
            }
        }
    }
}
