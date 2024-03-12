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
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer >= spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;

            if (platesSpawnedAmount < platesSpawnAmountMax)
            {
                platesSpawnedAmount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override bool CanInteract(Player player) => !player.HasKitchenObject && platesSpawnedAmount > 0;


    protected override void OnInteract(Player player)
    {
        KitchenObject.Spawn(plateKichenObjectSO, player);
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
