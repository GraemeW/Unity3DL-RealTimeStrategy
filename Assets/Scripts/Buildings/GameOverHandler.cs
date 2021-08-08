using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    // State
    private List<UnitBase> unitBases = new List<UnitBase>();

    // Events
    public static event Action<string> clientOnGameOver;

    #region Server
    public override void OnStartServer()
    {
        UnitBase.serverOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.serverOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.serverOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.serverOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        unitBases.Add(unitBase);
    }

    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        unitBases.Remove(unitBase);

        if (unitBases.Count != 1) { return; }

        int playerID = unitBases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerID}");
    }
    #endregion

    #region Client
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        if (clientOnGameOver != null)
        {
            clientOnGameOver.Invoke(winner);
        }
    }
    #endregion
}
