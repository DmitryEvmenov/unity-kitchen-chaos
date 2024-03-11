using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;

    public event EventHandler OnPlayerGrabbedObject;

    public override void Interact(Player player)
    {
        InstantiateNewKitchenObject();
        player.PickUpKitchenObject(GetKitchenObject());
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }

    private void InstantiateNewKitchenObject() =>
        Instantiate(KitchenObjectSO.prefab).GetComponent<KitchenObject>().SetParentKitchenObject(this);

    public override bool CanInteract(Player player) => !HasKitchenObject && !player.HasKitchenObject;
}
