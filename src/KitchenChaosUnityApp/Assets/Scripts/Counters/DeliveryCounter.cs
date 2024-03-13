using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    protected override void OnInteract(Player player) => player.GetKitchenObject().DestroySelf();

    public override bool CanInteract(Player player) => player.HasKitchenObject && player.GetKitchenObject().TryGetPlate(out var _);
}
