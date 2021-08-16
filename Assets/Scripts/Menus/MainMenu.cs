using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Tunables
    [SerializeField] GameObject landingPagePanel = null;
    [SerializeField] GameObject enterAddressPanel = null;

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost();
    }

    public void JoinLobby()
    {
        landingPagePanel.SetActive(false);
        enterAddressPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
