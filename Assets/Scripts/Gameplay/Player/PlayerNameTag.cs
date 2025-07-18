using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
public class PlayerNameTag : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText; // Displayed name ingame
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    private void Start()
    {
        if (isLocalPlayer)
        {
            CmdSetName(SteamFriends.GetPersonaName());

            // Hide name tag from self
            if (nameText != null)
            {
                nameText.gameObject.SetActive(false);
            }
        }
    }
    public void UpdateName(string newName)
    {
        nameText.text = newName;
    }

    void LateUpdate()
    {
        // Making sure the name always faces the camera
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
    [Command]
    private void CmdSetName(string name)
    {
        playerName = name;
    }

    private void OnNameChanged(string oldName, string newName)
    {
        UpdateName(newName);
    }
}
