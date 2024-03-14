using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private StoveCounter stoveCounter;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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
    }
}
