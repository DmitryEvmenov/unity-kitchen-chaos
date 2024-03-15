using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoveBurnWarningUI : MonoBehaviour
{
    [SerializeField] private Image warningImage;

    [SerializeField] private StoveCounter stoveCounter;

    private void Start()
    {
        HideWarning();

        stoveCounter.OnCookingStateChanged += StoveCounter_OnCookingStateChanged;
    }

    private void StoveCounter_OnCookingStateChanged(object sender, StoveCounter.OnCookingStateChangedEventArgs e)
    {
        if (e.cookingState == StoveCounter.CookingState.Burning)
        {
            ShowWarning();
        }
        else
        {
            HideWarning();
        }
    }

    private void ShowWarning() => warningImage.gameObject.SetActive(true);

    private void HideWarning() => warningImage.gameObject.SetActive(false);
}
