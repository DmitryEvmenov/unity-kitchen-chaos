using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectsArray;

    private void Start()
    {
        Player.OnAnyPlayerSpawned += Player_OnPlayerSpawned;
    }

    private void Player_OnPlayerSpawned(object sender, EventArgs e)
    {
        (sender as Player).OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show() => Array.ForEach(visualGameObjectsArray, vo => vo.SetActive(true));

    private void Hide() => Array.ForEach(visualGameObjectsArray, vo => vo.SetActive(false));
}
