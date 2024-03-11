using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;

    public override void Interact(Player player)
    {
        if (HasKitchenObject)
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
        else
        {
            if (player.HasKitchenObject)
            {
                player.DropKitchenObjectTo(this);
            }
        }
    }

    public override bool CanInteract(Player player) => HasKitchenObject || player.HasKitchenObject;
}
