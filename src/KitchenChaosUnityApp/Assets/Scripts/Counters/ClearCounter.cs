using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;

    protected override void OnInteract(Player player) => HandlePickUpPutDownInteraction(player);

    private void HandlePickUpPutDownInteraction(Player player)
    {
        if (HasKitchenObject)
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
        else if (player.HasKitchenObject)
        {
            player.PutDownKitchenObjectTo(this);
        }
    }

    public override bool CanInteract(Player player) => HasKitchenObject || player.HasKitchenObject;
}
