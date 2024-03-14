using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    protected override void OnInteract(Player player)
    {
        player.GetKitchenObject().TryGetPlate(out var plate);

        var delivered = DeliveryManager.Instance.TryDeliverOrder(plate);

        if (delivered)
        {
            player.GetKitchenObject().DestroySelf();
        }
    }

    public override bool CanInteract(Player player) => 
        player.HasKitchenObject && player.GetKitchenObject().TryGetPlate(out var _);
}
