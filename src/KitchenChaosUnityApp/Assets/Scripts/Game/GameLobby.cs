using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }

    private Lobby joinedLobby;

    private float heartbeatTimer;

    private const float HEARTBEAT_TIMER_MAX = 15f;

    private async void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeUnityAuthentication();
    }

    private void Start()
    {
        heartbeatTimer = HEARTBEAT_TIMER_MAX;
    }

    private async void Update()
    {
        await HandleHeartbeat();
    }

    private async Task HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = HEARTBEAT_TIMER_MAX;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    private async Task InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            var initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 1000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                GameMultiplayer.Instance.GetMaxPlayersCount(),
                new CreateLobbyOptions { IsPrivate = isPrivate });

            GameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task JoinCode(string code)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public string GetLobbyName() => joinedLobby?.Name ?? string.Empty;

    public string GetLobbyCode() => joinedLobby?.LobbyCode ?? string.Empty;
}
