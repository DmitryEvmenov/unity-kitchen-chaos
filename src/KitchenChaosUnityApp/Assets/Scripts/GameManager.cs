using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private State state;

    private bool isGamePaused = false;

    [SerializeField] private float countDownToStartTimer;
    [SerializeField] private float gamePlayingTimer;

    private float gamePlayingTimerMax;

    public event EventHandler<OnGameStateChangedEventArgs> OnGameStateChanged;

    public class OnGameStateChangedEventArgs : EventArgs
    {
        public State NewState { get; set; }
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private void Awake()
    {
        state = State.WaitingToStart;
        gamePlayingTimerMax = gamePlayingTimer;
        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnGameStateChanged?.Invoke(this, new OnGameStateChangedEventArgs { NewState = state });
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) => TogglePauseGame();

    private void Update()
    {
        switch (state)
        {
            case State.CountdownToStart:
                SwitchNextStateIfReady(State.GamePlaying, ref countDownToStartTimer);
                break;
            case State.GamePlaying:
                SwitchNextStateIfReady(State.GameOver, ref gamePlayingTimer);
                break;
            default:
                break;
        }
    }

    private void SwitchNextStateIfReady(State nextState, ref float currentStateTimer)
    {
        currentStateTimer -= Time.deltaTime;

        if (currentStateTimer <= 0)
        {
            state = nextState;
            OnGameStateChanged?.Invoke(this, new OnGameStateChangedEventArgs { NewState = state });
        }
    }

    public bool IsGamePlaying() => state == State.GamePlaying;

    public bool IsCountdownToStart() => state == State.CountdownToStart;

    public float GetCountdownToStartTimer() => countDownToStartTimer;

    public float GetGamePlayingTimerNormalized() => 1 - gamePlayingTimer / gamePlayingTimerMax;

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else 
        {
            Time.timeScale = 1;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
