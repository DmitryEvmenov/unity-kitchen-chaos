using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    [SerializeField] private int spawnPlateTimerMax;
    [SerializeField] private int platesSpawnAmountMax;
    [SerializeField] private KitchenObjectSO plateKichenObjectSO;

    private float spawnPlateTimer;
    private int platesSpawnedAmount;

    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        spawnPlateTimer -= Time.deltaTime;

        if (spawnPlateTimer <= 0f)
        {
            HandleSpawnVisualPlate();
        }
    }

    private void HandleSpawnVisualPlate()
    {
        spawnPlateTimer = spawnPlateTimerMax;

        if (platesSpawnedAmount < platesSpawnAmountMax)
        {
            platesSpawnedAmount++;

            OnPlateSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public override bool CanInteract(Player player) => !player.HasKitchenObject && platesSpawnedAmount > 0;

    protected override void OnInteract(Player player) => HandlePickUpPlateInteraction(player);

    private void HandlePickUpPlateInteraction(Player player)
    {
        var newPlate = KitchenObject.Spawn(plateKichenObjectSO);
        player.PickUpKitchenObject(newPlate);

        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
