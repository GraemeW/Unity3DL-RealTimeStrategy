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
    [SerializeField] Transform cameraTransform = null;

    // State
    [SyncVar (hook = nameof(ClientHandleResourcesUpdated))] int resources = 150;
    Color teamColor = new Color();
    List<Unit> units = new List<Unit>();
    List<Building> activeBuildings = new List<Building>();
    [SyncVar (hook = nameof(AuthorityHandlePartyOwnerStateUpdated))] bool isPartyOwner = false;
    [SyncVar (hook = nameof(AuthorityHandlePlayerStartPositionUpdated))] Vector3 playerStartPosition = new Vector3();
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))] string displayName = "";

    // Events
    public event Action<int> clientOnResourcesUpdated;
    public static event Action<bool> authorityOnPartyOwnerStateUpdated;
    public static event Action clientOnInfoUpdated;

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public bool IsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public string GetDisplayName()
    {
        return displayName;
    }

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

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetPartyOwner(bool enable)
    {
        isPartyOwner = enable;
    }

    [Server]
    public void SetPlayerStartPosition(Vector3 playerStartPosition)
    {
        this.playerStartPosition = playerStartPosition;
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    [Server]
    public void SetTeamColor(Color teamColor)
    {
        this.teamColor = teamColor;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner) { return; }

        NetworkManagerRTS networkManager = NetworkManager.singleton as NetworkManagerRTS;
        networkManager.StartGame();
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

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
        Unit.AuthorityOnUnitSpawned += ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += ClientHandleUnitDespawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }

        NetworkManagerRTS networkManagerRTS= NetworkManager.singleton as NetworkManagerRTS;
        if (networkManagerRTS != null)
        {
            networkManagerRTS.Players.Add(this);
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly) { return; }

        NetworkManagerRTS networkManagerRTS = NetworkManager.singleton as NetworkManagerRTS;
        if (networkManagerRTS != null)
        {
            networkManagerRTS.Players.Remove(this);
        }

        if (!hasAuthority) { return; }

        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
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

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        if (clientOnInfoUpdated != null)
        {
            clientOnInfoUpdated.Invoke();
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

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!hasAuthority) { return; }

        if (authorityOnPartyOwnerStateUpdated != null)
        {
            authorityOnPartyOwnerStateUpdated.Invoke(newState);
        }
    }

    private void AuthorityHandlePlayerStartPositionUpdated(Vector3 oldPlayerStartPosition, Vector3 newPlayerStartPosition)
    {
        if (!hasAuthority) { return; }

        Vector3 startPosition = new Vector3(
            newPlayerStartPosition.x,
            cameraTransform.position.y,
            newPlayerStartPosition.z);

        cameraTransform.position = startPosition;
    }

    #endregion
}
