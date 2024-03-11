using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public event EventHandler OnPlayerGrabbedObject;

    public override void Interact(Player player)
    {
        KitchenObject.Spawn(kitchenObjectSO, kitchenObjectParent: this);
        player.PickUpKitchenObject(GetKitchenObject());
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }

    public override bool CanInteract(Player player) => !HasKitchenObject && !player.HasKitchenObject;
}
