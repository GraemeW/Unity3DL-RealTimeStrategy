using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    // Tunables
    [SerializeField] private int maxHealth = 100;

    // State
    [SyncVar(hook = nameof(HandleHealthUpdated))] private int currentHealth;

    // Event
    public event Action serverOnDie;
    public event Action<int, int> clientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        UnitBase.serverOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.serverOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }

        if (serverOnDie != null)
        {
            serverOnDie.Invoke();
        }
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionID)
    {
        if (connectionID != connectionToClient.connectionId) { return; }

        DealDamage(currentHealth);
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        if (clientOnHealthUpdated != null)
        {
            clientOnHealthUpdated.Invoke(newHealth, maxHealth);
        }
    }

    #endregion
}
