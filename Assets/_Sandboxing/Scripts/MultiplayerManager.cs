using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using Fusion;
using System;
using System.Threading.Tasks;

public class MultiplayerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player data")]
    [SerializeField, ReadOnly] private string playerName;
    [SerializeField, ReadOnly] private string roomName;

    [Space]
    public NetworkRunner instanceRunner;
    public NetworkObject playerInstance;

    [Header("Fusion Components")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform defaultPossition;


    [Header("UI Game Objects")]
    public GameObject Canvas_playerName;
    public GameObject Canvas_RoomName;

    void Start()
    {
        // PlayGame();
    }

    public void InputPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        playerName = name;

        Canvas_playerName.SetActive(false);
        Canvas_RoomName.SetActive(true);
    }

    public void InputRoomName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        roomName = name;

        Canvas_RoomName.SetActive(false);
    }

    public void PlayGame()
    {
        StartGame();
    }

    async void StartGame()
    {
        if (instanceRunner == null)
        {
            instanceRunner = GetRunner("Runner");

            var result = await StartSimulation(instanceRunner, GameMode.Shared, roomName);

            if (result.Ok)
            {
                Debug.Log($"Has been entered {instanceRunner.SessionInfo.Name} room");
            }
            else
            {
                Destroy(instanceRunner);
                instanceRunner = null;
            }
        }
    }

    Task<StartGameResult> StartSimulation(NetworkRunner runner, GameMode gameMode, string room, HostMigrationToken migrationToken = null, Action<NetworkRunner> migrationResume = null)
    {
        return runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = room,
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            HostMigrationToken = migrationToken,
            HostMigrationResume = migrationResume,
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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player joined room");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left room");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"player exit from room: {runner.SessionInfo.Name}");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Player connected to server");

        if (runner.GameMode == GameMode.Shared)
        {
            Debug.Log($"{Time.time} Shared Mode - Spawning Player");
            StartCoroutine(InstantiatePlayer(runner, runner.LocalPlayer));
        }
    }

    IEnumerator InstantiatePlayer(NetworkRunner runner, PlayerRef playerref)
    {
        Debug.Log("try to spawn player...");

        yield return new WaitUntil(() => defaultPossition != null);

        var pos = UnityEngine.Random.insideUnitSphere * 5 + defaultPossition.position;
        pos.y = 1f;

        playerInstance = runner.Spawn(_playerPrefab, pos, defaultPossition.rotation, playerref, InitNetworkState);

        void InitNetworkState(NetworkRunner runner, NetworkObject obj)
        {
            // var networkPlayer = obj.GetBehaviour<NetworkPlayerManager>();
            // networkPlayer.Player = playerref;
            // networkPlayer.Name = playerName;

            runner.SetPlayerObject(playerref, obj);
            runner.SetPlayerAlwaysInterested(playerref, obj, true);

            Debug.Log("player has been spawned!!!");
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
