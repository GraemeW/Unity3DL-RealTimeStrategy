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
        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client

    #endregion
}
