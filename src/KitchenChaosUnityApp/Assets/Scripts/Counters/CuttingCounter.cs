using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    public static event EventHandler OnAnyCut;

    public static new void ResetStaticData()
    {
        OnAnyCut = null;
    }

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
                    KitchenObject.Destroy(kitchenObject);
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

        ResetCuttingProgressServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetCuttingProgressServerRpc() => ResetCuttingProgressClientRpc();

    [ClientRpc]
    private void ResetCuttingProgressClientRpc() => ResetCuttingProgress();

    protected override void OnInteractAlternate(Player player) => HandleCuttingInteraction();

    private void HandleCuttingInteraction()
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

        CutObjectServerRpc();
        CheckCuttingProgressDoneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc() => CutObjectClientRpc();

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        var currentKitchenObject = GetKitchenObject();
        cuttingProgress++;

        var recipeSO = GetCuttingRecipeSO(currentKitchenObject.KitchenObjectSO);

        NotifyUpdateCuttingProgressServerRpc(recipeSO.cuttingProgressMax);
        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckCuttingProgressDoneServerRpc()
    {
        var currentKitchenObject = GetKitchenObject();
        var recipeSO = GetCuttingRecipeSO(currentKitchenObject.KitchenObjectSO);

        if (cuttingProgress >= recipeSO.cuttingProgressMax)
        {
            KitchenObject.Destroy(currentKitchenObject);
            KitchenObject.Spawn(recipeSO.output, kitchenObjectParent: this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyUpdateCuttingProgressServerRpc(int cuttingProgressMax = 1) => NotifyUpdateCuttingProgressClientRpc(cuttingProgressMax);

    [ClientRpc]
    private void NotifyUpdateCuttingProgressClientRpc(int cuttingProgressMax = 1) => NotifyUpdateCuttingProgress(cuttingProgressMax);

    private void NotifyUpdateCuttingProgress(int cuttingProgressMax = 1) =>
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { progressNormalized = (float)cuttingProgress / cuttingProgressMax });


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
        NotifyUpdateCuttingProgressServerRpc();
    }
}
