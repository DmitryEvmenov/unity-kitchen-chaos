using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private StoveCounter stoveCounter;

    [SerializeField] private float warningSoundTimerMax = .2f;

    private bool playWarningSound = false;
    private float warningSoundTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        warningSoundTimer = warningSoundTimerMax;
    }

    private void Start()
    {
        stoveCounter.OnCookingStateChanged += StoveCounter_OnCookingStateChanged;
    }

    private void StoveCounter_OnCookingStateChanged(object sender, StoveCounter.OnCookingStateChangedEventArgs e)
    {
        var playSound = e.cookingState != StoveCounter.CookingState.Idle;

        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }

        playWarningSound = e.cookingState == StoveCounter.CookingState.Burning;
    }

    private void Update()
    {
        if (!playWarningSound)
        {
            return;
        }

        warningSoundTimer -= Time.deltaTime;
        if (warningSoundTimer <= 0)
        {
            warningSoundTimer = warningSoundTimerMax;

            SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
        }
    }
}
