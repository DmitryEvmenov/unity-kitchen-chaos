using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

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

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject)
        {
            var currentKitchenObject = GetKitchenObject();
            if (!HasValidRecipeFor(currentKitchenObject.KitchenObjectSO))
            {
                return;
            }

            var objectToSpawn = GetOutputForInput(currentKitchenObject.KitchenObjectSO);

            currentKitchenObject.DestroySelf();
            KitchenObject.Spawn(objectToSpawn, kitchenObjectParent: this);
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) => 
        cuttingRecipeSOArray.First(recipes => recipes.input == inputKitchenObjectSO).output;


    public override bool CanInteract(Player player) => 
        HasKitchenObject 
            || (player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasValidRecipeFor(KitchenObjectSO kitchenObjectSO) => cuttingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);
}
