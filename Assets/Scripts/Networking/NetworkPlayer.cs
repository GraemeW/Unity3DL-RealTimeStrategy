using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkPlayer : NetworkBehaviour
{
    // Tunables
    [SerializeField] Building[] buildings = new Building[0];
    [SerializeField] LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] float buildingRangeLimit = 5f;

    // State
    [SyncVar (hook = nameof(ClientHandleResourcesUpdated))] int resources = 500;
    List<Unit> units = new List<Unit>();
    List<Building> activeBuildings = new List<Building>();

    // Events
    public event Action<int> clientOnResourcesUpdated;

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 position)
    {
        if (Physics.CheckBox(position + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building in activeBuildings)
        {
            if ((position - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false;
    }

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 position)
    {
        Building buildingToPlace = null;
        foreach (Building building in buildings)
        {
            if (building.GetID() == buildingID)
            {
                buildingToPlace = building;
                break;
            }
        }
        if (buildingToPlace == null) { return; }
        if (buildingToPlace.GetPrice() > resources) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        if (!CanPlaceBuilding(buildingCollider, position)) { return; }

        GameObject unitInstance = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);
        unitInstance.transform.SetParent(null);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        SetResources(GetResources() - buildingToPlace.GetPrice());
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        units.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        units.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        activeBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        activeBuildings.Remove(building);
    }
    #endregion

    #region Client
    public List<Unit> GetUnits()
    {
        return units;
    }

    public List<Building> GetBuildings()
    {
        return activeBuildings;
    }

    public int GetResources()
    {
        return resources;
    }

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += ClientHandleUnitDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= ClientHandleUnitDespawned;
    }

    private void ClientHandleUnitSpawned(Unit unit)
    {
        units.Add(unit);
    }

    private void ClientHandleUnitDespawned(Unit unit)
    {
        units.Remove(unit);
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        if (clientOnResourcesUpdated != null)
        {
            clientOnResourcesUpdated.Invoke(newResources);
        }
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        activeBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        activeBuildings.Remove(building);
    }

    #endregion
}
