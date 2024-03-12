using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    [SerializeField] private StovingRecipeSO[] stovingRecipeSOArray;

    private int cookingProgressTimeSeconds;
    private CookingState cookingState;

    public enum CookingState
    {
        NoCooking,
        Cooking,
        Burning
    }

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnCookingStateChangedEventArgs> OnCookingStateChanged;

    public class OnCookingStateChangedEventArgs : EventArgs
    {
        public CookingState cookingState;
    }

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

    public override void InteractAlternate(Player player) => HandleCookingInteraction(player);

    private void HandleCookingInteraction(Player player)
    {
        if (!HasKitchenObject || !HasValidRecipeFor(GetKitchenObject().KitchenObjectSO))
        {
            return;
        }

        switch (cookingState)
        {
            case CookingState.NoCooking:
                cookingState = CookingState.Cooking;
                cookingProgressTimeSeconds++;
                NotifyUpdateCuttingProgress(5);
                break;
            case CookingState.Cooking:
                cookingState = CookingState.Burning;
                cookingProgressTimeSeconds++;
                NotifyUpdateCuttingProgress(3);
                break;
            case CookingState.Burning:
                cookingState = CookingState.NoCooking;
                cookingProgressTimeSeconds = 0;
                NotifyUpdateCuttingProgress();
                break;
        }

        NotifyCookingStateChanged();
    }

    public override bool CanInteract(Player player) =>
            HasKitchenObject
                || (player.HasKitchenObject && HasValidRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasValidRecipeFor(KitchenObjectSO kitchenObjectSO) =>
        stovingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);

    private void ResetCookingProgress()
    {
        cookingProgressTimeSeconds = 0;
        cookingState = CookingState.NoCooking;
        NotifyUpdateCuttingProgress();
        NotifyCookingStateChanged();
    }

    private void NotifyUpdateCuttingProgress(int? cookingProgressMaxSeconds = null) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = (float)cookingProgressTimeSeconds / cookingProgressMaxSeconds ?? 1 });

    private void NotifyCookingStateChanged() => OnCookingStateChanged?.Invoke(this, new OnCookingStateChangedEventArgs { cookingState = cookingState });
}
