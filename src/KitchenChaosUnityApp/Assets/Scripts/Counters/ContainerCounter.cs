using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public event EventHandler OnPlayerGrabbedObject;

    protected override void OnInteract(Player player) => HandleSpawnNewPickUpInteraction(player);

    private void HandleSpawnNewPickUpInteraction(Player player)
    {
        KitchenObject.Spawn(kitchenObjectSO, player);

        InteractLogicServerRpc();
    }

    public override bool CanInteract(Player player) => !player.HasKitchenObject;

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc() => InteractLogicClientRpc();

    [ClientRpc]
    private void InteractLogicClientRpc() => OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
}
