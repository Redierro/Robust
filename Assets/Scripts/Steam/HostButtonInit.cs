using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class HostButtonInit : MonoBehaviour
    {
        public void StartServer()
        {
            SteamLobbySC.Instance.gameObject.SetActive(true); // Reactivate if disabled
            SteamLobbySC.Instance.HostLobby(); // Begin Steam lobby setup
        }
    }

}