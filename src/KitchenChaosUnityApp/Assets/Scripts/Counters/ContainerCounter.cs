using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public event EventHandler OnPlayerGrabbedObject;

    protected override void OnInteract(Player player) => HandleSpawnNewPickUpInteraction(player);

    private void HandleSpawnNewPickUpInteraction(Player player)
    {
        var newSpawnedObject = KitchenObject.Spawn(kitchenObjectSO);

        player.PickUpKitchenObject(newSpawnedObject);

        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }

    public override bool CanInteract(Player player) => !player.HasKitchenObject;
}
