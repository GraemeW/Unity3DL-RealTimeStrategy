using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class NetworkManagerRTS : NetworkManager
{
    // Tunables
    [SerializeField] GameObject basePrefab = null;
    [SerializeField] GameOverHandler gameOverHandlerPrefab = null;
    [SerializeField] string gameSceneReference = "Scene_Map01";

    // State
    public List<NetworkPlayer> Players { get; } = new List<NetworkPlayer>();
    private bool isGameInProgress = false;

    // Events
    public static event Action clientOnConnected;
    public static event Action clientOnDisconnected;

    #region Server
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        NetworkPlayer networkPlayer = conn.identity.GetComponent<NetworkPlayer>();
        Players.Add(networkPlayer);

        networkPlayer.SetDisplayName(string.Format("Player {0}", Players.Count.ToString()));

        Color playerColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        networkPlayer.SetTeamColor(playerColor);

        networkPlayer.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (NetworkPlayer networkPlayer in Players)
            {
                Transform playerStartTransform = GetStartPosition();
                networkPlayer.SetPlayerStartPosition(playerStartTransform.position);
                GameObject baseInstance = Instantiate(basePrefab, playerStartTransform.position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, networkPlayer.connectionToClient);
            }

        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress || Players.Count >= 4) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkPlayer networkPlayer = conn.identity.GetComponent<NetworkPlayer>();
        Players.Remove(networkPlayer);
        networkPlayer.SetDisplayName("Disconnecting . . .");

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (Players.Count < 2) { return; }

        isGameInProgress = true;
        ServerChangeScene(gameSceneReference);
    }
    #endregion

    #region Client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        if (clientOnConnected != null)
        {
            clientOnConnected.Invoke();
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        if (clientOnDisconnected != null)
        {
            clientOnDisconnected.Invoke();
        }
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }
    #endregion



}
