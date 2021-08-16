using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{
    // Tunables
    [SerializeField] GameObject landingPagePanel = null;
    [SerializeField] TMP_InputField addressInput = null;
    [SerializeField] Button joinButton = null;

    private void OnEnable()
    {
        NetworkManagerRTS.clientOnConnected += HandleClientConnected;
        NetworkManagerRTS.clientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerRTS.clientOnConnected -= HandleClientConnected;
        NetworkManagerRTS.clientOnDisconnected -= HandleClientDisconnected;
    }

    public void ExitAddressPanel()
    {
        joinButton.interactable = true;
        landingPagePanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Join()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        landingPagePanel.SetActive(false);
        gameObject.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
