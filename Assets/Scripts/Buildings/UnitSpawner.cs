using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    // Tunables
    [SerializeField] Health health = null;
    [SerializeField] Unit unitPrefab = null;
    [SerializeField] Transform spawnLocation = null;
    [SerializeField] TextMeshProUGUI remainingUnitText = null;
    [SerializeField] Image unitProgressImage = null;
    [SerializeField] int maxUnitQueue = 5;
    [SerializeField] float spawnMoveRange = 7f;
    [SerializeField] float unitSpawnDuration = 5f;

    // State
    [SyncVar (hook = nameof(ClientHandleQueuedUnitsUpdated))] int queuedUnits = 0;
    [SyncVar] float unitTimer = 0f;
    float progressImageVelocity;

    // Cached References
    NetworkPlayer networkPlayer = null;

    private void Update()
    {
        SetUpNetworkPlayerReference();

        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    private void SetUpNetworkPlayerReference()
    {
        // Called after Start
        // Race Condition:  Cannot guarantee client is available within start since it follows from networkmanager Start() routine
        if (networkPlayer == null)
        {
            NetworkConnection networkConnection = NetworkClient.connection;
            if (networkConnection != null)
            {
                NetworkIdentity networkIdentity = networkConnection.identity;
                if (networkIdentity != null) { networkPlayer = networkIdentity.GetComponent<NetworkPlayer>(); }
            }
        }
    }

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

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits <= 0) { return; }

        unitTimer += Time.deltaTime;
        if (unitTimer < unitSpawnDuration) { return; }

        Unit unitInstance = Instantiate(unitPrefab, spawnLocation);
        unitInstance.transform.SetParent(null);
        NetworkServer.Spawn(unitInstance.gameObject, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnLocation.position.y;
        UnitMover unitMover = unitInstance.GetComponent<UnitMover>();
        unitMover.ServerMove(spawnLocation.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits >= maxUnitQueue) { return; }
        if (networkPlayer.GetResources() < unitPrefab.GetResourceCost()) { return; }

        networkPlayer.SetResources(networkPlayer.GetResources() - unitPrefab.GetResourceCost());
        queuedUnits++;
    }
    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAuthority) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitText.text = newUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float newProgress = Mathf.Clamp(unitTimer / unitSpawnDuration, 0f, 1f);

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion

}
