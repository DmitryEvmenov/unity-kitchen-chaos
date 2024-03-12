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
            var kitchenObject = GetKitchenObject();

            if (player.HasKitchenObject && player.GetKitchenObject().TryGetPlate(out var plate))
            {
                if (kitchenObject.TryGetPlate(out var secondPlate))
                {
                    player.PickUpKitchenObject(secondPlate);
                }
                else if (plate.TryAddIngredient(kitchenObject.KitchenObjectSO))
                {
                    kitchenObject.DestroySelf();
                }
                else
                {
                    player.PickUpKitchenObject(GetKitchenObject());
                }
            }
            else if (player.HasKitchenObject && kitchenObject.TryGetPlate(out plate))
            {
                var playerObject = player.GetKitchenObject();

                if (plate.TryAddIngredient(playerObject.KitchenObjectSO))
                {
                    playerObject.DestroySelf();
                }
                else
                {
                    player.PickUpKitchenObject(GetKitchenObject());
                }
            }
            else
            {
                player.PickUpKitchenObject(GetKitchenObject());
            }
        }
        else if (player.HasKitchenObject)
        {
            player.PutDownKitchenObjectTo(this);
        }
    }

    public override bool CanInteract(Player player) => HasKitchenObject || player.HasKitchenObject;
}
