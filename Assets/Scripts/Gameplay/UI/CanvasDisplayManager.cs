using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SteamLobby
{
    public class CanvasDisplayManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerCanvas;

        private void Update()
        {
            if (isLocalPlayer)
                playerCanvas.SetActive(true);
            else
                playerCanvas.SetActive(false);
        }
    }
}
