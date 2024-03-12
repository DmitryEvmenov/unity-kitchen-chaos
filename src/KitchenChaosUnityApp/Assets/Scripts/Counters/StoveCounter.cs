using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    [SerializeField] private StovingRecipeSO[] stovingRecipeSOArray;

    private int cookingProgressTimeSeconds;

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    protected override void OnInteract(Player player) => HandlePickUpPutDownInteraction(player);

    private void HandlePickUpPutDownInteraction(Player player)
    {
        var hasValidRecipeForPlayerObject = player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO);

        if (HasKitchenObject && (!player.HasKitchenObject || hasValidRecipeForPlayerObject))
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
        else if (hasValidRecipeForPlayerObject)
        {
            player.PutDownKitchenObjectTo(this);
        }

        ResetCookingProgress();
    }

    public override void InteractAlternate(Player player)
    {
        base.InteractAlternate(player);
    }

    public override bool CanInteract(Player player) =>
            HasKitchenObject
                || (player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasValidRecipeFor(KitchenObjectSO kitchenObjectSO) =>
        stovingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);

    private void ResetCookingProgress()
    {
        cookingProgressTimeSeconds = 0;
        NotifyUpdateCuttingProgress();
    }

    private void NotifyUpdateCuttingProgress(int? cookingProgressTimePassedSeconds = null) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = (float)cookingProgressTimeSeconds / cookingProgressTimePassedSeconds ?? 1 });
}
