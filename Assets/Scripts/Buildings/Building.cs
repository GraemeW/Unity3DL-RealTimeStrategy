using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : NetworkBehaviour
{
    // Tunables
    [SerializeField] GameObject buildingPreview = null;
    [SerializeField] Sprite icon = null;
    [SerializeField] int id = -1;
    [SerializeField] int price = 100;

    // Events
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }
    public Sprite GetIcon()
    {
        return icon;
    }

    public int GetID()
    {
        return id;
    }

    public int GetPrice()
    {
        return price;
    }

    #region Server
    public override void OnStartServer()
    {
        if (ServerOnBuildingSpawned != null)
        {
            ServerOnBuildingSpawned.Invoke(this);
        }
    }

    public override void OnStopServer()
    {
        if (ServerOnBuildingDespawned != null)
        {
            ServerOnBuildingDespawned.Invoke(this);
        }
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (AuthorityOnBuildingSpawned != null)
        {
            AuthorityOnBuildingSpawned.Invoke(this);
        }
    }

    public override void OnStopClient()
    {
        if (hasAuthority && AuthorityOnBuildingDespawned != null)
        {
            AuthorityOnBuildingDespawned.Invoke(this);
        }
    }

    #endregion
}
