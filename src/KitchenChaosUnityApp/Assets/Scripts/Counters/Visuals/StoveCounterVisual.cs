using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private GameObject[] visualGameObjectsArray;

    void Start()
    {
        stoveCounter.OnCookingStateChanged += StoveCounter_OnCookingStateChanged;
    }

    private void StoveCounter_OnCookingStateChanged(object sender, StoveCounter.OnCookingStateChangedEventArgs e)
    {
        switch (e.cookingState)
        {
            case StoveCounter.CookingState.Idle:
                Hide();
                break;
            case StoveCounter.CookingState.Cooking:
            case StoveCounter.CookingState.Burning:
            case StoveCounter.CookingState.Spoiled:
                Show();
                break;
            default:
                Hide(); 
                break;
        }
    }

    private void Show() => Array.ForEach(visualGameObjectsArray, vo => vo.SetActive(true));

    private void Hide() => Array.ForEach(visualGameObjectsArray, vo => vo.SetActive(false));
}
