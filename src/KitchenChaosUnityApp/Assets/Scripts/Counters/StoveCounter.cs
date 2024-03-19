using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    [SerializeField] private StovingRecipeSO[] stovingRecipeSOArray;

    private float cookingProgressTimer;
    private CookingState cookingState;

    public enum CookingState
    {
        Idle,
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
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject && (cookingState == CookingState.Cooking || cookingState == CookingState.Burning))
        {
            cookingProgressTimer += Time.deltaTime;

            var kitchenObjectSO = GetKitchenObject().KitchenObjectSO; 
            var recipeSO = GetOutputRecipeSOByState(kitchenObjectSO);

            if (Doing(CookingState.Cooking, recipeSO.secondsToCook))
            {
                NotifyUpdateCookingProgressServerRpc(cookingProgressTimer, recipeSO.secondsToCook);
            }
            else if (Done(CookingState.Cooking, recipeSO.secondsToCook))
            {
                HandleStateChangeServerRpc(CookingState.Burning);
            }
            else if (Doing(CookingState.Burning, recipeSO.secondsToBurnAfterCooked))
            {
                NotifyUpdateCookingProgressServerRpc(cookingProgressTimer, recipeSO.secondsToBurnAfterCooked, ProgressState.Warning);
            }
            else if (Done(CookingState.Burning, recipeSO.secondsToBurnAfterCooked))
            {
                HandleStateChangeServerRpc(CookingState.Spoiled);
            }

            bool Doing(CookingState state, int maxSeconds) => cookingState == state && cookingProgressTimer < maxSeconds;
            bool Done(CookingState state, int maxSeconds) => cookingState == state && cookingProgressTimer >= maxSeconds;
        }
    }

    private StovingRecipeSO GetOutputRecipeSOByState(KitchenObjectSO kitchenObjectSO) => cookingState == CookingState.Cooking
                ? GetStovingRecipeSO(kitchenObjectSO)
                : GetStovingRecipeSOForCooked(kitchenObjectSO);

    private KitchenObjectSO GetOutputKitchenObjectSOByState(KitchenObjectSO kitchenObjectSO) => cookingState == CookingState.Cooking
        ? GetOutputRecipeSOByState(kitchenObjectSO).outputWhenCooked
        : GetOutputRecipeSOByState(kitchenObjectSO).outputWhenBurned;

    [ServerRpc(RequireOwnership = false)]
    private void HandleStateChangeServerRpc(CookingState newState)
    {
        var currentKitchenObject = GetKitchenObject();
        var output = GetOutputKitchenObjectSOByState(currentKitchenObject.KitchenObjectSO);

        KitchenObject.Destroy(currentKitchenObject);
        KitchenObject.Spawn(output, this);

        HandleStateChangeClientRpc(newState);
    }

    [ClientRpc]
    private void HandleStateChangeClientRpc(CookingState newState)
    {
        cookingState = newState;
        cookingProgressTimer = 0;
        NotifyCookingStateChanged();
        NotifyUpdateCookingProgress();
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
                    KitchenObject.Destroy(kitchenObject);
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

        ResetCookingProgressServerRpc();
    }

    protected override void OnInteractAlternate(Player player) => HandleCookingInteraction();

    private void HandleCookingInteraction()
    {
        if ((!HasKitchenObject || !HasAnyValidRecipeFor(GetKitchenObject().KitchenObjectSO)) && cookingState != CookingState.Spoiled)
        {
            return;
        }

        HandleCookingStateChangeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleCookingStateChangeServerRpc() => HandleCookingStateChangeClientRpc();

    [ClientRpc]
    private void HandleCookingStateChangeClientRpc()
    {
        var kitchenObjectSO = GetKitchenObject().KitchenObjectSO;

        switch (cookingState)
        {
            case CookingState.Idle:
                var hasCookingRecipe = HasValidCookingRecipeFor(kitchenObjectSO);

                cookingState = hasCookingRecipe
                    ? CookingState.Cooking
                    : CookingState.Burning;
                break;
            case CookingState.Cooking:
            case CookingState.Burning:
            case CookingState.Spoiled:
                cookingState = CookingState.Idle;
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
        cookingState = CookingState.Idle;
        NotifyUpdateCookingProgress();
        NotifyCookingStateChanged();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetCookingProgressServerRpc() => ResetCookingProgressClientRpc();

    [ClientRpc]
    private void ResetCookingProgressClientRpc() => ResetCookingProgress();

    private void NotifyUpdateCookingProgress(float progressTimer = 0, int cookingProgressMaxSeconds = 1, ProgressState progressState = ProgressState.Regular) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = progressTimer / cookingProgressMaxSeconds, progressState = progressState });

    [ServerRpc(RequireOwnership = false)]
    private void NotifyUpdateCookingProgressServerRpc(float progressTimer = 0, int cookingProgressMaxSeconds = 1, ProgressState progressState = ProgressState.Regular) =>
        NotifyUpdateCookingProgressClientRpc(progressTimer, cookingProgressMaxSeconds, progressState);

    [ClientRpc]
    private void NotifyUpdateCookingProgressClientRpc(float progressTimer = 0, int cookingProgressMaxSeconds = 1, ProgressState progressState = ProgressState.Regular) =>
        NotifyUpdateCookingProgress(progressTimer, cookingProgressMaxSeconds, progressState);

    private void NotifyCookingStateChanged() => OnCookingStateChanged?.Invoke(this, new OnCookingStateChangedEventArgs { cookingState = cookingState });
}
