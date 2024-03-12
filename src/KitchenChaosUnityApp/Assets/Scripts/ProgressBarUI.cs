using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnProgressChangedEventArgs : EventArgs
{
    public float progressNormalized;
}

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    // todo: this is a workaround solution to let pass interface type as serializedField
    [SerializeField] GameObject progressibleCounterGameObject;
    private IHasProgress progressibleCounter => progressibleCounterGameObject.GetComponent<IHasProgress>();

    private void Start()
    {
        progressibleCounter.OnProgressChanged += ProgressibleCounter_OnProgressChanged;

        barImage.fillAmount = 0f;

        Hide();
    }

    private void ProgressibleCounter_OnProgressChanged(object sender, OnProgressChangedEventArgs e) 
    {
        barImage.fillAmount = e.progressNormalized;

        if (e.progressNormalized == 0f || e.progressNormalized == 1f)
        {
            Hide();
        } 
        else
        {
            Show();
        }
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
