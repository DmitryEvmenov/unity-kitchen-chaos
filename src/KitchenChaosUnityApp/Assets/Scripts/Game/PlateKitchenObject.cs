using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectSOList;

    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;

    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO KitchenObjectSO;
    }


    protected override void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
        base.Awake();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (kitchenObjectSOList.Contains(kitchenObjectSO) || !validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;

        }

        AddIngredientServerRpc(GameMultiplayer.Instance.GetKitcheObjectSOIndex(kitchenObjectSO));

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex) => AddIngredientClientRpc(kitchenObjectSOIndex);

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        var kitchenObjectSO = GameMultiplayer.Instance.GetKitchenObjectSOByIndex(kitchenObjectSOIndex);

        kitchenObjectSOList.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { KitchenObjectSO = kitchenObjectSO });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList() => kitchenObjectSOList;
}
