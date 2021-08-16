using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    // Tunables
    [SerializeField] GameObject lobbyUI = null;
    [SerializeField] GameObject landingPagePanel = null;
    [SerializeField] Button startGameButton = null;
    [SerializeField] TMP_Text[] playerNameTexts = new TMP_Text[4];

    // State
    NetworkPlayer networkPlayer = null;

    private void OnEnable()
    {
        NetworkManagerRTS.clientOnConnected += HandleClientConnected;
        NetworkManagerRTS.clientOnDisconnected += HandleClientDisconnected;
        NetworkPlayer.authorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        NetworkPlayer.clientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDisable()
    {
        NetworkManagerRTS.clientOnConnected -= HandleClientConnected;
        NetworkManagerRTS.clientOnDisconnected -= HandleClientDisconnected;
        NetworkPlayer.authorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        NetworkPlayer.clientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void HandleClientDisconnected()
    {
        LeaveLobby();
    }

    private void ClientHandleInfoUpdated()
    {
        NetworkManagerRTS networkManagerRTS = NetworkManager.singleton as NetworkManagerRTS;
        List<NetworkPlayer> players = networkManagerRTS.Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }

        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting for Player . . .";
        }

        startGameButton.interactable = players.Count >= 2;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool enable)
    {
        startGameButton.gameObject.SetActive(enable);
    }

    public void StartGame()
    {
        SetUpNetworkPlayerReference();
        networkPlayer.CmdStartGame();
    }

    private void SetUpNetworkPlayerReference()
    {
        if (networkPlayer == null)
        {
            NetworkConnection networkConnection = NetworkClient.connection;
            if (networkConnection != null)
            {
                NetworkIdentity networkIdentity = networkConnection.identity;
                if (networkIdentity != null) { networkPlayer = networkIdentity.GetComponent<NetworkPlayer>(); }
            }
        }
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
        landingPagePanel.SetActive(true);
        lobbyUI.SetActive(false);
        SceneManager.LoadScene(0);
    }
}
