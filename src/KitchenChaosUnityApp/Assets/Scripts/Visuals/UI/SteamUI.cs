using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamUI : MonoBehaviour
{
    [SerializeField] StoveCounter stoveCounter;
    [SerializeField] Color cookingColor;
    [SerializeField] float cookingSize;
    [SerializeField] Color burningColor;
    [SerializeField] float burningSize;

    private ParticleSystem particle => gameObject.GetComponent<ParticleSystem>();

    private void Start()
    {
        Hide();
        stoveCounter.OnCookingStateChanged += StoveCounter_OnCookingStateChanged;
    }

    private void StoveCounter_OnCookingStateChanged(object sender, StoveCounter.OnCookingStateChangedEventArgs e)
    {
        switch (e.cookingState)
        {
            case StoveCounter.CookingState.Cooking:
                SetCookingMode();
                Show();
                break;
            case StoveCounter.CookingState.Burning:
            case StoveCounter.CookingState.Spoiled:
                SetBurningMode();
                Show();
                break;
            default:
                Hide();
                break;
        }
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);

    private void SetCookingMode()
    {
        var mainParticle = particle.main;
        mainParticle.startColor = cookingColor;
        mainParticle.startSize = cookingSize;
    }

    private void SetBurningMode()
    {
        var mainParticle = particle.main;
        mainParticle.startColor = burningColor;
        mainParticle.startSize = burningSize;
    }
}
