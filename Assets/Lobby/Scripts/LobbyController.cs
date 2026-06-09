using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("Multiplayer (opcional)")]
    [SerializeField] private KarstConnectionManager connectionManager;
    [SerializeField] private bool connectOnEnter = true;

    [Header("Game Transition")]
    [SerializeField] private int gameSceneIndex = 2;

    private bool isConnecting;
    private bool connectionAttempted;

    private async void Start()
    {
        if (connectOnEnter && connectionManager != null)
        {
            isConnecting = true;
            connectionAttempted = true;
            await TryConnect();
            isConnecting = false;
        }
    }

    private async Task TryConnect()
    {
        try
        {
            await connectionManager.Connect();
        }
        catch (System.Exception e)
        {
            Debug.Log($"[Lobby] Conexao multiplayer falhou (single-player disponivel): {e.Message}");
        }
    }

    public void EnterGame()
    {
        if (isConnecting) return;

        SceneTransitionManager.singleton.GoToScene(gameSceneIndex);
    }

    public async void RetryConnection()
    {
        if (connectionManager != null && !isConnecting)
        {
            isConnecting = true;
            await TryConnect();
            isConnecting = false;
        }
    }
}
