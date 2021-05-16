using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class Unit : NetworkBehaviour
{
    [SerializeField] UnitMover unitMover = null;
    [SerializeField] Targeter targeter = null;
    [SerializeField] UnityEvent onSelected = null;
    [SerializeField] UnityEvent onDeselected = null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public UnitMover GetUnitMover()
    {
        return unitMover;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server
    [Server]
    public override void OnStartServer()
    {
        if (ServerOnUnitSpawned != null)
        {
            ServerOnUnitSpawned.Invoke(this);
        }
    }

    [Server]
    public override void OnStopServer()
    {
        if (ServerOnUnitDespawned != null)
        {
            ServerOnUnitDespawned.Invoke(this);
        }
    }
    #endregion

    #region Client
    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }

        if (onSelected != null)
        {
            onSelected.Invoke();
        }
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) { return; }

        if (onDeselected != null)
        {
            onDeselected.Invoke();
        }
    }

    [Client]
    public override void OnStartClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        if (AuthorityOnUnitSpawned != null)
        {
            AuthorityOnUnitSpawned.Invoke(this);
        }
    }

    [Client]
    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        if (AuthorityOnUnitDespawned != null)
        {
            AuthorityOnUnitDespawned.Invoke(this);
        }
    }
    #endregion
}
