using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private bool isLocalPlayerReady;

    private Dictionary<ulong, bool> playersReadyDictionary;
    private Dictionary<ulong, bool> playersPausedDictionary;
    private bool autoUpdateGamePausedState;

    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new(false);


    [SerializeField] private NetworkVariable<float> countDownToStartTimer = new();
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new();

    private float gamePlayingTimerMax;

    public event EventHandler<OnGameStateChangedEventArgs> OnGameStateChanged;

    public class OnGameStateChangedEventArgs : EventArgs
    {
        public State NewState { get; set; }
    }

    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    private void Awake()
    {
        gamePlayingTimerMax = gamePlayingTimer.Value;
        playersReadyDictionary = new Dictionary<ulong, bool>();
        playersPausedDictionary = new Dictionary<ulong, bool>();
        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoUpdateGamePausedState = true;
    }

    private void LateUpdate()
    {
        if (autoUpdateGamePausedState)
        {
            autoUpdateGamePausedState = false;
            UpdateGamePausedState();
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnGameStateChanged?.Invoke(this, new OnGameStateChangedEventArgs { NewState = state.Value });
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playersReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        if (CheckIfAllConnectedPlayersReady())
        {
            state.Value = State.CountdownToStart;
        }
    }

    private bool CheckIfAllConnectedPlayersReady()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playersReadyDictionary.ContainsKey(clientId) || !playersReadyDictionary[clientId])
                return false;
        }
        return true;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) => TogglePauseGame();

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.CountdownToStart:
                SwitchNextStateIfReady(State.GamePlaying, countDownToStartTimer);
                break;
            case State.GamePlaying:
                SwitchNextStateIfReady(State.GameOver, gamePlayingTimer);
                break;
            default:
                break;
        }
    }

    private void SwitchNextStateIfReady(State nextState, NetworkVariable<float> currentStateTimer)
    {
        try
        {
            currentStateTimer.Value -= Time.deltaTime;

            if (currentStateTimer.Value <= 0)
            {
                state.Value = nextState;
            }
        }
        catch (NullReferenceException)
        {
            // todo: weird MarkNetworkBehaviorDirty tied to how the networkvariable being disposed on app quite?, gonna have to resolve later
        }
    }

    public bool IsGamePlaying() => state.Value == State.GamePlaying;

    public bool IsCountdownToStart() => state.Value == State.CountdownToStart;

    public float GetCountdownToStartTimer() => countDownToStartTimer.Value;

    public float GetGamePlayingTimerNormalized() => 1 - gamePlayingTimer.Value / gamePlayingTimerMax;

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;

        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else 
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playersPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        UpdateGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playersPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        UpdateGamePausedState();
    }

    private void UpdateGamePausedState()
    {
        isGamePaused.Value = NetworkManager.Singleton.ConnectedClientsIds.Any(c => playersPausedDictionary.ContainsKey(c) && playersPausedDictionary[c]);
    }

    public bool IsLocalPlayerReady() => isLocalPlayerReady;
}
