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
        networkPlayer = connectionToClient.identity.GetComponent<NetworkPlayer>();

        teamColor = networkPlayer.GetTeamColor();
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        HandleTeamColorUpdated(teamColor, teamColor);
    }

    private void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach (Renderer renderer in colorRenderers)
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    #endregion

}
