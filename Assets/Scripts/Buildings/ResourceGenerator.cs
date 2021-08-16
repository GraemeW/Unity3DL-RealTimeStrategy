using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    // Tunables
    [SerializeField] Health health = null;
    [SerializeField] int resourcesPerInterval = 10;
    [SerializeField] float interval = 2f;

    // State
    float timer = 0f;
    private NetworkPlayer networkPlayer = null;

    public override void OnStartServer()
    {
        timer = interval;
        networkPlayer = connectionToClient.identity.GetComponent<NetworkPlayer>();

        health.serverOnDie += ServerHandleDie;
        GameOverHandler.serverOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.serverOnDie -= ServerHandleDie;
        GameOverHandler.serverOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            networkPlayer.SetResources(networkPlayer.GetResources() + resourcesPerInterval);

            timer += interval;
        }
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;
    }
}
