using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    [SerializeField] private StovingRecipeSO[] stovingRecipeSOArray;

    private float cookingProgressTimer;
    private CookingState cookingState;

    public enum CookingState
    {
        NoCooking,
        Cooking,
        Burning,
        Spoiled,
    }

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnCookingStateChangedEventArgs> OnCookingStateChanged;

    public class OnCookingStateChangedEventArgs : EventArgs
    {
        public CookingState cookingState;
    }

    private void Update()
    {
        if (HasKitchenObject && (cookingState == CookingState.Cooking || cookingState == CookingState.Burning))
        {
            cookingProgressTimer += Time.deltaTime;

            var kitchenObject = GetKitchenObject();
            var recipeSO = cookingState == CookingState.Cooking
                ? GetStovingRecipeSO(kitchenObject.KitchenObjectSO)
                : GetStovingRecipeSOForCooked(kitchenObject.KitchenObjectSO);


            if (Doing(CookingState.Cooking, recipeSO.secondsToCook))
            {
                NotifyUpdateCookingProgress(cookingProgressTimer, recipeSO.secondsToCook);
            }
            else if (Done(CookingState.Cooking, recipeSO.secondsToCook))
            {
                HandleStateChange(CookingState.Burning, kitchenObject, recipeSO.outputWhenCooked);
            }
            else if (Doing(CookingState.Burning, recipeSO.secondsToBurnAfterCooked))
            {
                NotifyUpdateCookingProgress(cookingProgressTimer, recipeSO.secondsToBurnAfterCooked, Color.red);
            }
            else if (Done(CookingState.Burning, recipeSO.secondsToBurnAfterCooked))
            {
                HandleStateChange(CookingState.Spoiled, kitchenObject, recipeSO.outputWhenBurned);
            }

            bool Doing(CookingState state, int maxSeconds) => cookingState == state && cookingProgressTimer < maxSeconds;
            bool Done(CookingState state, int maxSeconds) => cookingState == state && cookingProgressTimer >= maxSeconds;
        }
    }

    private void HandleStateChange(CookingState newState, KitchenObject currentKitchenObject, KitchenObjectSO newObjectToSpawnSO)
    {
        cookingState = newState;
        cookingProgressTimer = 0;
        NotifyCookingStateChanged();
        NotifyUpdateCookingProgress();
        currentKitchenObject.DestroySelf();
        KitchenObject.Spawn(newObjectToSpawnSO, this);
    }

    protected override void OnInteract(Player player) => HandlePickUpPutDownInteraction(player);

    private void HandlePickUpPutDownInteraction(Player player)
    {
        var hasValidCookingRecipeForPlayerObject = player.HasKitchenObject && HasValidCookingRecipeFor(player.GetKitchenObject().KitchenObjectSO);

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
            else if (!player.HasKitchenObject || hasValidCookingRecipeForPlayerObject)
            {
                player.PickUpKitchenObject(GetKitchenObject());
            }
        }
        else if (hasValidCookingRecipeForPlayerObject)
        {
            player.PutDownKitchenObjectTo(this);
        }

        ResetCookingProgress();
    }

    public override void InteractAlternate(Player player) => HandleCookingInteraction();

    private void HandleCookingInteraction()
    {
        if ((!HasKitchenObject || !HasAnyValidRecipeFor(GetKitchenObject().KitchenObjectSO)) && cookingState != CookingState.Spoiled)
        {
            return;
        }

        var kitchenObjectSO = GetKitchenObject().KitchenObjectSO;

        switch (cookingState)
        {
            case CookingState.NoCooking:
                var hasCookingRecipe = HasValidCookingRecipeFor(kitchenObjectSO);

                cookingState = hasCookingRecipe
                    ? CookingState.Cooking
                    : CookingState.Burning;
                break;
            case CookingState.Cooking:
            case CookingState.Burning:
            case CookingState.Spoiled:
                cookingState = CookingState.NoCooking;
                break;
        }

        NotifyCookingStateChanged();
    }

    public override bool CanInteract(Player player) =>
            HasKitchenObject
                || (player.HasKitchenObject && HasValidCookingRecipeFor(player.GetKitchenObject().KitchenObjectSO));

    private bool HasAnyValidRecipeFor(KitchenObjectSO kitchenObjectSO) =>
        stovingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO || recipes.outputWhenCooked == kitchenObjectSO);

    private bool HasValidCookingRecipeFor(KitchenObjectSO kitchenObjectSO) =>
        stovingRecipeSOArray.Any(recipes => recipes.input == kitchenObjectSO);

    private StovingRecipeSO GetStovingRecipeSO(KitchenObjectSO inputKitchenObjectSO) =>
        stovingRecipeSOArray.FirstOrDefault(recipes => recipes.input == inputKitchenObjectSO);

    private StovingRecipeSO GetStovingRecipeSOForCooked(KitchenObjectSO cookedKitchenObjectSO) =>
        stovingRecipeSOArray.FirstOrDefault(recipes => recipes.outputWhenCooked == cookedKitchenObjectSO);

    private void ResetCookingProgress()
    {
        cookingProgressTimer = 0;
        cookingState = CookingState.NoCooking;
        NotifyUpdateCookingProgress();
        NotifyCookingStateChanged();
    }

    private void NotifyUpdateCookingProgress(float progressTimer = 0, int? cookingProgressMaxSeconds = null, Color? color = null) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = progressTimer / cookingProgressMaxSeconds ?? 1, progressColor = color });

    private void NotifyCookingStateChanged() => OnCookingStateChanged?.Invoke(this, new OnCookingStateChangedEventArgs { cookingState = cookingState });
}
