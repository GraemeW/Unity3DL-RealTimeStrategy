using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    // Tunables
    [SerializeField] TextMeshProUGUI resourcesText = null;

    // State
    bool subscribedToResources = false;

    // Cached References
    NetworkPlayer networkPlayer = null;

    private void Update()
    {
        SetUpNetworkPlayerReference();
        SubscribeToPlayerResources(true);
    }

    private void OnEnable()
    {
        SubscribeToPlayerResources(true);
    }

    private void OnDisable()
    {
        SubscribeToPlayerResources(false);
    }

    private void SubscribeToPlayerResources(bool enable)
    {
        if (networkPlayer == null || subscribedToResources) { return; }

        if (enable)
        {
            ClientHandleResourcesUpdated(networkPlayer.GetResources());

            networkPlayer.clientOnResourcesUpdated += ClientHandleResourcesUpdated;
            subscribedToResources = true;
        }
        else
        {
            networkPlayer.clientOnResourcesUpdated -= ClientHandleResourcesUpdated;
            subscribedToResources = false;
        }
    }

    private void SetUpNetworkPlayerReference()
    {
        // Called after Start
        // Race Condition:  Cannot guarantee client is available within start since it follows from networkmanager Start() routine
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

    private void ClientHandleResourcesUpdated(int newResourceValue)
    {
        resourcesText.text = string.Format("Resources: {0}", newResourceValue.ToString());
    }
}
