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
                HandlePlayerHoldsPlateInteraction(player, plate, kitchenObject);
            }
            else if (player.HasKitchenObject && kitchenObject.TryGetPlate(out plate))
            {
                HandleCounterHoldsPlateInteraction(player, plate);
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

    private void HandlePlayerHoldsPlateInteraction(Player player, PlateKitchenObject plate, KitchenObject kitchenObject)
    {
        if (kitchenObject.TryGetPlate(out var secondPlate))
        {
            player.PickUpKitchenObject(secondPlate);
        }
        else if (plate.TryAddIngredient(kitchenObject.KitchenObjectSO))
        {
            KitchenObject.Destroy(kitchenObject);
        }
        else
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
    }

    private void HandleCounterHoldsPlateInteraction(Player player, PlateKitchenObject plate)
    {
        var playerObject = player.GetKitchenObject();

        if (plate.TryAddIngredient(playerObject.KitchenObjectSO))
        {
            KitchenObject.Destroy(playerObject);
        }
        else
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
    }
}
