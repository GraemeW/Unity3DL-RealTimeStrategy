using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    // Tunables
    [SerializeField] Health health = null;

    // Events
    public static event Action<UnitBase> serverOnBaseSpawned;
    public static event Action<UnitBase> serverOnBaseDespawned;
    public static event Action<int> serverOnPlayerDie;

    #region Server
    public override void OnStartServer()
    {
        health.serverOnDie += ServerHandleDie;
        if (serverOnBaseSpawned != null)
        {
            serverOnBaseSpawned.Invoke(this);
        }
    }

    public override void OnStopServer()
    {
        health.serverOnDie -= ServerHandleDie;
        if (serverOnBaseDespawned != null)
        {
            serverOnBaseDespawned.Invoke(this);
        }
    }

    [Server]
    private void ServerHandleDie()
    {
        if (serverOnPlayerDie != null)
        {
            serverOnPlayerDie.Invoke(connectionToClient.connectionId);
        }

        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client

    #endregion
}
