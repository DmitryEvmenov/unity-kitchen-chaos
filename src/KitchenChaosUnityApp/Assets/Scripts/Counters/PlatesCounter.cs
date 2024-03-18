using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        if (!IsServer || !GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        spawnPlateTimer -= Time.deltaTime;

        if (spawnPlateTimer <= 0f)
        {
            spawnPlateTimer = spawnPlateTimerMax;

            if (platesSpawnedAmount < platesSpawnAmountMax)
            {
                SpawnPlateServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc() => SpawnPlateClientRpc();

    [ClientRpc]
    private void SpawnPlateClientRpc() => HandleSpawnVisualPlate();

    private void HandleSpawnVisualPlate()
    {
        platesSpawnedAmount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override bool CanInteract(Player player) => !player.HasKitchenObject && platesSpawnedAmount > 0;

    protected override void OnInteract(Player player) => HandlePickUpPlateInteraction(player);

    private void HandlePickUpPlateInteraction(Player player)
    {
        KitchenObject.Spawn(plateKichenObjectSO, player);

        InteractLogicServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc() => InteractLogicClientRpc();

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
