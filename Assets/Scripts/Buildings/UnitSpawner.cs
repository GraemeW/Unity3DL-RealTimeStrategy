using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    // Tunables
    [SerializeField] Health health = null;
    [SerializeField] GameObject unitPrefab = null;
    [SerializeField] Transform spawnLocation = null;

    #region Server

    public override void OnStartServer()
    {
        health.serverOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.serverOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, spawnLocation);
        unitInstance.transform.SetParent(null);
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }
    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAuthority) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        CmdSpawnUnit();
    }


    #endregion

}
