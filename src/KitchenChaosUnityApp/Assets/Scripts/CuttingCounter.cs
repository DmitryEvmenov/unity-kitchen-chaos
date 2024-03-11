using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }

    public event EventHandler OnCut;

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

                cuttingProgress = 0;
                NotifyUpdateCuttingProgress();
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

            cuttingProgress++;

            var recipeSO = GetCuttingRecipeSO(currentKitchenObject.KitchenObjectSO);

            NotifyUpdateCuttingProgress(recipeSO.cuttingProgressMax);
            OnCut?.Invoke(this, EventArgs.Empty);

            if (cuttingProgress >= recipeSO.cuttingProgressMax)
            {
                currentKitchenObject.DestroySelf();
                KitchenObject.Spawn(recipeSO.output, kitchenObjectParent: this);
            }
        }
    }

    private void NotifyUpdateCuttingProgress(int? cuttingProgressMax = null) => 
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = (float)cuttingProgress / cuttingProgressMax ?? 1 });


    public override bool CanInteract(Player player) => 
        HasKitchenObject 
            || (player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasValidRecipeFor(KitchenObjectSO kitchenObjectSO) => 
        cuttingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);

    private CuttingRecipeSO GetCuttingRecipeSO(KitchenObjectSO inputKitchenObjectSO) =>
        cuttingRecipeSOArray.First(recipes => recipes.input == inputKitchenObjectSO);
}
