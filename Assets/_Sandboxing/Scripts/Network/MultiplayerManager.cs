using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using Fusion;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private static MultiplayerManager Instance;

    [Header("Player data")]

#if UNITY_EDITOR
    [ReadOnly]
#endif
    [SerializeField]
    private string playerName;

#if UNITY_EDITOR
    [ReadOnly]
#endif
    [SerializeField]
    private string roomName;

    [Space]
    public NetworkRunner instanceRunner;
    public NetworkObject networkPlayerController;
    PlayerInputHandler playerInputHandler;

    [Header("Fusion Components")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform defaultPossition;
    private byte[] _connectionToken;


    // [Header("UI Game Objects")]
    // public GameObject Canvas_playerName;
    // public GameObject Canvas_RoomName;

    [Header("Connection Event")]
    public UnityEvent OnPlayerConnectToServer;
    public UnityEvent OnPlayerOnShutdownFromServer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (_connectionToken == null)
                Instance._connectionToken = ConnectionTokenUtils.NewToken();
        }
    }

    void Start()
    {
        // PlayGame();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && instanceRunner) instanceRunner.Shutdown();
    }

    public void InputPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        playerName = name;

        // Canvas_playerName.SetActive(false);
        // Canvas_RoomName.SetActive(true);
    }

    public void InputRoomName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        roomName = name;

        // Canvas_RoomName.SetActive(false);
    }

    public void PlayGame()
    {
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning($"can't enter the room, please fill the room name");
            return;
        }

        StartGame();
    }

    async void StartGame()
    {
        if (instanceRunner == null)
        {
            instanceRunner = GetRunner("Runner");

            // var result = await StartSimulation(instanceRunner, GameMode.AutoHostOrClient, roomName, , NetAddress.Any());
            var result = await StartSimulation(instanceRunner, GameMode.Shared, roomName, _connectionToken, NetAddress.Any());

            if (result.Ok)
            {
                Debug.Log($"Network-Runner has been instantiated");
            }
            else
            {
                Destroy(instanceRunner);
                instanceRunner = null;
            }
        }
    }

    Task<StartGameResult> StartSimulation(NetworkRunner runner, GameMode gameMode, string room, byte[] connectionToken, NetAddress address, HostMigrationToken migrationToken = null, Action<NetworkRunner> initialized = null)
    {
        return runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Address = address,
            SessionName = room,
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            Initialized = initialized,
            HostMigrationToken = migrationToken,
            ConnectionToken = connectionToken
        });
    }

    NetworkRunner GetRunner(string name)
    {
        var runner = Instantiate(runnerPrefab);
        runner.name = name;
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        return runner;
    }

    int GetPlayerToken(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)
        {
            return ConnectionTokenUtils.HashToken(_connectionToken);
        }
        else
        {
            var token = runner.GetPlayerConnectionToken(player);

            if (token != null)
            {
                return ConnectionTokenUtils.HashToken(token);
            }
        }

        return 0; // invalid
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (playerInputHandler != null)
        {
            input.Set(playerInputHandler.GetNetworkInput());
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("there is player left room");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown");

        if (Application.isPlaying && shutdownReason != ShutdownReason.HostMigration)
        {
            Destroy(instanceRunner);
            instanceRunner = null;
            networkPlayerController = null;
        }

        OnPlayerOnShutdownFromServer.Invoke();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Player connected to server");
        OnPlayerConnectToServer.Invoke();

        if (runner.GameMode == GameMode.Shared)
        {
            StartCoroutine(InstantiatePlayer(runner, runner.LocalPlayer));
        }
    }

    IEnumerator InstantiatePlayer(NetworkRunner runner, PlayerRef playerref)
    {
        Debug.Log($"{Time.time} try to spawn player...");

        yield return new WaitUntil(() => defaultPossition != null);

        var _playerToken = GetPlayerToken(runner, playerref);

        networkPlayerController = runner.Spawn(_playerPrefab, Utils.GetRandomSpawnPoint(defaultPossition), defaultPossition.rotation, playerref, InitNetworkState);

        void InitNetworkState(NetworkRunner runner, NetworkObject obj)
        {
            var networkPlayer = obj.GetBehaviour<NetworkPlayerControllerPC>();
            networkPlayer.RPC_SetNickname(playerName);
            networkPlayer.playerToken = _playerToken;

            playerInputHandler = networkPlayer.GetComponent<PlayerInputHandler>();

            runner.SetPlayerObject(playerref, obj);
            runner.SetPlayerAlwaysInterested(playerref, obj, true);
        }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        runner.Shutdown();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        runner.Shutdown();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
