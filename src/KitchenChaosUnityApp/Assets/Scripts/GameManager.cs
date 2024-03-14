using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private float waitingToStartTimer;
    [SerializeField] private float countDownToStartTimer;
    [SerializeField] private float gamePlayingTimer;

    public event EventHandler<OnGameStateChangedEventArgs> OnGameStateChanged;

    public class OnGameStateChangedEventArgs : EventArgs
    {
        public State NewState { get; set; }
    }

    private void Awake()
    {
        state = State.WaitingToStart;
        Instance = this;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                SwitchNextStateIfReady(State.CountdownToStart, ref waitingToStartTimer);
                break;
            case State.CountdownToStart:
                SwitchNextStateIfReady(State.GamePlaying, ref countDownToStartTimer);
                break;
            case State.GamePlaying:
                SwitchNextStateIfReady(State.GameOver, ref gamePlayingTimer);
                break;
            case State.GameOver:
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
}
