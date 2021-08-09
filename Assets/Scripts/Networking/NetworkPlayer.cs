using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    // State
    List<Unit> units = new List<Unit>();
    List<Building> buildings = new List<Building>();

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

        buildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        buildings.Remove(building);
    }
    #endregion

    #region Client
    public List<Unit> GetUnits()
    {
        return units;
    }

    public List<Building> GetBuildings()
    {
        return buildings;
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

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        buildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        buildings.Remove(building);
    }

    #endregion
}
