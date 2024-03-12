using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    protected override void OnInteract(Player player) => HandlePickUpPutDownInteraction(player);

    private void HandlePickUpPutDownInteraction(Player player)
    {
        var hasValidRecipeForPlayerObject = player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO);

        if (HasKitchenObject)
        {
            if (player.HasKitchenObject && player.GetKitchenObject().TryGetPlate(out var plate))
            {
                var kitchenObject = GetKitchenObject();

                if (plate.TryAddIngredient(kitchenObject.KitchenObjectSO))
                {
                    kitchenObject.DestroySelf();
                }
                else
                {
                    return;
                }
            }
            else if (!player.HasKitchenObject || hasValidRecipeForPlayerObject)
            {
                player.PickUpKitchenObject(GetKitchenObject());
            }
        }
        else if (hasValidRecipeForPlayerObject)
        {
            player.PutDownKitchenObjectTo(this);
        }

        ResetCuttingProgress();
    }

    protected override void OnInteractAlternate(Player player) => HandleCuttingInteraction(player);

    private void HandleCuttingInteraction(Player player)
    {
        if (!HasKitchenObject)
        {
            return;
        }

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

    private void NotifyUpdateCuttingProgress(int? cuttingProgressMax = null) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = (float)cuttingProgress / cuttingProgressMax ?? 1 });


    public override bool CanInteract(Player player) =>
        HasKitchenObject
            || (player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasValidRecipeFor(KitchenObjectSO kitchenObjectSO) =>
        cuttingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);

    private CuttingRecipeSO GetCuttingRecipeSO(KitchenObjectSO inputKitchenObjectSO) =>
        cuttingRecipeSOArray.First(recipes => recipes.input == inputKitchenObjectSO);

    private void ResetCuttingProgress()
    {
        cuttingProgress = 0;
        NotifyUpdateCuttingProgress();
    }
}
