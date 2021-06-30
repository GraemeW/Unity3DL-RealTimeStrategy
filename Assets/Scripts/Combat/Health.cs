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
    [SyncVar]
    private int currentHealth;

    // Event
    public event Action serverOnDie;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
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

        Debug.Log($"{gameObject.name} died");
    }

    #endregion

    #region Client

    #endregion
}
