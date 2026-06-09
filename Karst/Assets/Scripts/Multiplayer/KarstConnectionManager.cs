using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fase 1 do coop: inicia uma sessao Fusion em modo Shared (sala fixa) e
/// faz o spawn em rede do prefab do jogador quando cada cliente entra.
///
/// Coloque este componente num GameObject vazio (ex.: "Network") na cena de gameplay.
/// Arraste o prefab do jogador (com NetworkObject) no campo playerPrefab.
/// </summary>
public class KarstConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Sessao")]
    [Tooltip("Nome fixo da sala. Todos que usam o mesmo nome caem na mesma sessao.")]
    [SerializeField] private string roomName = "KarstCoop";

    [Tooltip("Maximo de jogadores na sala.")]
    [SerializeField] private int maxPlayers = 4;

    [Tooltip("Conecta automaticamente ao iniciar a cena.")]
    [SerializeField] private bool connectOnStart = true;

    [Header("Jogador")]
    [Tooltip("Prefab do jogador. Precisa ter um NetworkObject.")]
    [SerializeField] private NetworkObject playerPrefab;

    [Tooltip("Pontos de spawn opcionais. Se vazio, usa a posicao deste objeto.")]
    [SerializeField] private Transform[] spawnPoints;

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;

    private async void Start()
    {
        if (connectOnStart) await Connect();
    }

    public async Task Connect()
    {
        if (runner == null)
        {
            runner = gameObject.GetComponent<NetworkRunner>();
            if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
        }
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var sceneInfo = new NetworkSceneInfo();
        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        if (sceneRef.IsValid) sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            PlayerCount = maxPlayers,
            Scene = sceneInfo,
            SceneManager = sceneManager,
        });

        if (result.Ok)
            Debug.Log($"[Karst] Conectado na sala '{roomName}' (Shared).");
        else
            Debug.LogError($"[Karst] Falha ao conectar: {result.ShutdownReason}");
    }

    // Em modo Shared, cada cliente faz o spawn do SEU proprio jogador.
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer || playerPrefab == null) return;

        var spawn = GetSpawnPose(player);
        runner.Spawn(playerPrefab, spawn.position, spawn.rotation, player);
        Debug.Log($"[Karst] Jogador local {player} entrou e foi spawnado.");
    }

    private (Vector3 position, Quaternion rotation) GetSpawnPose(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var t = spawnPoints[Mathf.Abs(player.AsIndex) % spawnPoints.Length];
            if (t != null) return (t.position, t.rotation);
        }
        return (transform.position, transform.rotation);
    }

    // ---- INetworkRunnerCallbacks (vazios; necessarios pela interface) ----
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
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
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
