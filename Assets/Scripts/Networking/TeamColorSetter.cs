using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    // Tunables
    [SerializeField] Renderer[] colorRenderers = new Renderer[0];

    // State
    [SyncVar (hook = nameof(HandleTeamColorUpdated))] Color teamColor = new Color();

    // Cached References
    NetworkPlayer networkPlayer = null;

    #region Server
    public override void OnStartServer()
    {
        SetUpNetworkPlayerReference();

        teamColor = networkPlayer.GetTeamColor();
    }
    #endregion

    #region Client
    private void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach (Renderer renderer in colorRenderers)
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    #endregion

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
}
