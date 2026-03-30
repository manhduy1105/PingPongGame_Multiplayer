using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

// ghi nhận các input của người chơi, ở đây chỉ có hướng di chuyển


public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner { get; set; }

    public LobbyManager LobbyManager;
    

    private bool _ballStarted = false;

    private void Awake()
    {
       
        // dont destroy this object when loading new scenes
        if(_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();
        DontDestroyOnLoad(gameObject);
    }
    
    // thông tin profile của player local, sẽ được tạo ra từ lobby và gửi lên host để tạo player object, ở đây tạm thời chỉ tạo 1 profile mặc định
    public PlayerProfile LocalPlayerProfile { get; private set; }
    public void SetLocalPlayerProfile(PlayerProfile profile)
    {
        LocalPlayerProfile = profile;
    }
    
    // start lobby
    public async Task StartLobby()
    {
        if(_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
        if (result.Ok)
        {
            Debug.Log("Joined lobby successfully!");
        }
        else
        {
            Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
        }
    }
    
    // tạo phòng
    public async Task StartHost(string sessionName, SceneRef scene)
    {
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            Scene = scene,
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        if (result.Ok)
        {
            Debug.Log("Started host successfully!");
        }
        else
        {
            Debug.LogError($"Failed to start host: {result.ShutdownReason}");
        }
    }
    
    // tham gia phòng
    public async Task StartClient(string sessionName)
    {
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        if (result.Ok)        {
            Debug.Log("Started client successfully!");
        }
        else        {
            Debug.LogError($"Failed to start client: {result.ShutdownReason}");
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }
    
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // nếu là host
            if (runner.IsServer)
            {
                Vector2 spawnPosition;

                if (player == runner.LocalPlayer)
                {
                    spawnPosition = new Vector2(-8, 0);
                }
                else
                {
                    spawnPosition = new Vector2(8, 0);
                }

                
            // Create a unique position for the player
            // var spawnPosition = new Vector2(Random.Range(0, 5), Random.Range(0, 5));
            var networkPlayerObject = runner.Spawn(
                _playerPrefab, 
                spawnPosition, 
                Quaternion.identity, 
                player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            // if (runner.ActivePlayers.Count() == 2 && !_ballStarted)
            // {
            //     var ball = FindFirstObjectByType<Ball>();
            //     ball.Launch();
            //     _ballStarted = true;
            // }
            
        }

        Debug.Log("Player joined: " + player + ", is local player: " + (player == runner.LocalPlayer));
        if (player == runner.LocalPlayer)
        {
            var pdm = FindFirstObjectByType<PlayerDataManager>();
            if (pdm == null) return;
            var metaData = new PlayerMetaData()
            {
                Name = LocalPlayerProfile.Name,
                Class = LocalPlayerProfile.Class,
                
            };
            pdm.RPC_UpdatePlayerMetaData(player, metaData);
        }
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
    
    

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // collect the input and send it to the runner, which will then be sent to the server and other clients
        var data = new PlayerInputData();
        data.direction = Input.GetAxis("Vertical");
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    // khi có update về danh sách phòng,
    // ví dụ khi có phòng mới được tạo hoặc phòng bị xóa,
    // host sẽ gửi update này cho tất cả client trong lobby
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Session list updated, total sessions: " + sessionList.Count);
        LobbyManager.DisplayRoomList(sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void DespawnGO(NetworkObject networkObject)
    {
        if (_runner != null && networkObject != null)
        {
            _runner.Despawn(networkObject);
        }
    }

    public NetworkObject SpawnGO(NetworkPrefabRef prefabRef, Vector2 position,Quaternion rotation, PlayerRef owner = default)
    {
        if (_runner != null)
        {
            return _runner.Spawn(prefabRef, position, rotation, owner);
        }
        return null;
    }
}
