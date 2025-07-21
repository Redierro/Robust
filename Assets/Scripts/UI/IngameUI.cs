using UnityEngine;
using Mirror;
using Steamworks;

namespace SteamLobby
{
    public class IngameUI : NetworkBehaviour
    {
        public GameObject escapeMenu;
        public bool escapeRaised = false;
        public void OpenEscape()
        {
            if (!escapeRaised && !ChatManager.Instance.chatRaised)
            {
                escapeMenu.SetActive(true);
                escapeRaised = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (escapeRaised)
            {
                escapeMenu.SetActive(false);
                escapeRaised = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        public void OnLeaveGamePressed()
        {
            if (CustomNetworkManager.singleton != null)
            {
                CustomNetworkManager.singleton.LeaveGameToLobby();
            }
        }

    }

}
