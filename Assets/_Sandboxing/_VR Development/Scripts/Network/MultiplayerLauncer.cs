using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using Fusion;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Seville.Multiplayer.Launcer
{
    public class MultiplayerLauncer : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static MultiplayerLauncer Instance;
        [SerializeField] private bool playOnStart = false;

        [Header("UI Lobby")]
        public GameObject Canvas_playerName;
        public GameObject Canvas_RoomName;
        [Space]

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
        public NetworkPlayer instancePlayer;

        [Space]
        [Header("Fusion Components")]
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private NetworkPlayer networkPlayerPrefab;
        [SerializeField] private Transform defaultPossition;

        [Space]
        [Header("Connection Event")]
        [Space]
        public UnityEvent OnPlayerConnectToServer;
        [Space]
        public UnityEvent OnPlayerShutdownFromServer;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            if (playOnStart == true)
            {
                playerName = "player tester";
                roomName = "1";

                StartGame();
            }
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

            PlayGame();
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
                var result = await StartSimulation(instanceRunner, GameMode.Shared, roomName, NetAddress.Any());

                if (result.Ok)
                {
                    Debug.Log($"Connected....");
                }
                else
                {
                    Destroy(instanceRunner);
                    instanceRunner = null;
                    Debug.LogWarning($"{result.ErrorMessage}");
                }
            }
        }

        Task<StartGameResult> StartSimulation(NetworkRunner runner, GameMode gameMode, string room, NetAddress address, Action<NetworkRunner> initialized = null)
        {
            return runner.StartGame(new StartGameArgs()
            {
                GameMode = gameMode,
                Address = address,
                SessionName = room,
                SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Initialized = initialized,
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

        IEnumerator InstantiatePlayer(NetworkRunner runner, PlayerRef playerref)
        {
            Debug.Log($"{Time.time} try to spawn player...");

            yield return new WaitUntil(() => defaultPossition != null);

            instancePlayer = runner.Spawn(networkPlayerPrefab, Utils.GetRandomSpawnPoint(defaultPossition), defaultPossition.rotation,
                                            playerref, InitNetworkState);

            void InitNetworkState(NetworkRunner runner, NetworkObject obj)
            {
                var networkPlayer = obj.GetBehaviour<NetworkPlayer>();
                networkPlayer.RPC_SetNickname(playerName);
                // instancePlayer.RPC_SetNickname(playerName);
                // instancePlayer.shouldRigTransform = instancePlayer.transform;

                runner.SetPlayerObject(playerref, obj);
                runner.SetPlayerAlwaysInterested(playerref, obj, true);
            }
        }

        // START: Photon Fusion NetworkEvents
        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Player connected to server");
            OnPlayerConnectToServer.Invoke();

            if (runner.GameMode == GameMode.Shared)
            {
                StartCoroutine(InstantiatePlayer(runner, runner.LocalPlayer));
            }
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"OnShutdown");

            if (Application.isPlaying && shutdownReason != ShutdownReason.HostMigration)
            {
                Destroy(instanceRunner);
                instanceRunner = null;
                instancePlayer = null;
            }

            OnPlayerShutdownFromServer.Invoke();
        }
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            runner.Shutdown();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            runner.Shutdown();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("there is player join room");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("there is player left room");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        // END: Photon Fusion Network Event
    }
}