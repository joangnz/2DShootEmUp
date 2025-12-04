using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private AttackManager attackManager;
    [SerializeField] private NetworkPrefabRef _playerPrefab, _mobPrefab;
    public static Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private bool _mouseButton0, _mouseButton1;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            // Vector2 spawnPosition = new((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, new(0, 0), Quaternion.identity, playerRef);

            // Run a player method that, in that client, updates all the existing player's states to the server's
            Player player = networkPlayerObject.GetComponent<Player>();

            // Create logic for availability
            int characterId = characterManager.GetFirstAvailableCharacterId();
            if (characterId < 1 || characterId > 5) return;

            characterManager.SetCharacterAvailable(characterId, false);
            PlayerState PlayerStateRef = new(characterId, 100);
            player.UpdatePlayerState(PlayerStateRef);
            player.SetAnimator(characterManager.GetAnimator(characterId));
            player.SetSprite(characterManager.GetSprite(characterId));
            player.RPC_ChangeCharacter(characterId);

            NotifyNewPlayerOfExistingPlayers(playerRef);

            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(playerRef, networkPlayerObject);
        }
    }

    public void NotifyNewPlayerOfExistingPlayers(PlayerRef newPlayeRef)
    {
        foreach (PlayerRef playerRef in _spawnedCharacters.Keys.ToList())
        {
            var playerObject = _spawnedCharacters[playerRef];
            Player player = playerObject.GetComponent<Player>();

            player.RPC_UpdatePlayerInfo(newPlayeRef);
        }
    }

    public void KillPlayer(Player player)
    {
        if (_spawnedCharacters.TryGetValue(player.Object.InputAuthority, out NetworkObject networkObject))
        {
            characterManager.SetCharacterAvailable(player.GetPlayerState().GetCharacterId(), true);
            _spawnedCharacters.Remove(player.Object.InputAuthority);
            _runner.Despawn(networkObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            characterManager.SetCharacterAvailable(networkObject.GetComponent<Player>().GetPlayerState().GetCharacterId(), true);
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    private void Update()
    {
        _mouseButton0 |= Input.GetMouseButton(0);
        _mouseButton1 |= Input.GetMouseButton(1);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
        {
            data.direction += Vector2.up;
            data.north = true;
            data.south = false;
        }
        if (Input.GetKey(KeyCode.S))
        {
            data.direction += Vector2.down;
            data.south = true;
            data.north = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            data.direction += Vector2.left;
            data.west = true;
            data.east = false;
        }
        if (Input.GetKey(KeyCode.D))
        {
            data.direction += Vector2.right;
            data.east = true;
            data.west = false;
        }

        if (Input.GetKey(KeyCode.Alpha1)) data.buttons.Set(NetworkInputData.NUMBER1, true);
        else if (Input.GetKey(KeyCode.Alpha2)) data.buttons.Set(NetworkInputData.NUMBER2, true);
        else if (Input.GetKey(KeyCode.Alpha3)) data.buttons.Set(NetworkInputData.NUMBER3, true);
        else if (Input.GetKey(KeyCode.Alpha4)) data.buttons.Set(NetworkInputData.NUMBER4, true);
        else if (Input.GetKey(KeyCode.Alpha5)) data.buttons.Set(NetworkInputData.NUMBER5, true);

        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        _mouseButton0 = _mouseButton1 = false;

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    private NetworkRunner _runner;
    private Coroutine _mobSpawnerCoroutine;

    private IEnumerator SpawnMobs()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (!_runner.IsServer) break;
            float y = UnityEngine.Random.Range(-6f, 6f);
            float x = UnityEngine.Random.Range(-9f, -5f);
            Vector2 spawnPosition = new(x, y);
            _ = _runner.Spawn(_mobPrefab, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(5f);
        }
    }

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (mode != GameMode.Server) _mobSpawnerCoroutine = StartCoroutine(SpawnMobs());
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Server")) StartGame(GameMode.Server);
            if (GUI.Button(new Rect(0, 40, 200, 40), "Host")) StartGame(GameMode.Host);
            if (GUI.Button(new Rect(0, 80, 200, 40), "Join")) StartGame(GameMode.Client);
        }
    }
}
