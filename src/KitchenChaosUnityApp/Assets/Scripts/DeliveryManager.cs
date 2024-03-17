using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance;

    private List<RecipeSO> waitingRecipeSOList;

    private float spawnRecipeTimer;

    private int successfulDeliveriesAmount;


    [SerializeField] private int spawnRecipeTimerMaxSeconds;

    [SerializeField] private int waitingRecipeListMax;

    [SerializeField] private RecipeListSO recipeListSO;

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnDeliverySuccess;
    public event EventHandler OnDeliveryFailure;

    private void Awake()
    {
        waitingRecipeSOList = new List<RecipeSO>();
        successfulDeliveriesAmount = 0;
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer || !GameManager.Instance.IsGamePlaying() || waitingRecipeSOList.Count >= waitingRecipeListMax)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMaxSeconds;

            var waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

            SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        var waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public bool TryDeliverOrder(PlateKitchenObject plateKitchenObject)
    {
        var found = false;
        var deliveredRecipeIndex = -1;
        var plateKitcheObjectsSOList = plateKitchenObject.GetKitchenObjectSOList();
        

        foreach (var (waitingRecipe, index) in waitingRecipeSOList.Select((item, index) => (item, index)))
        {
            found = AreEqual(waitingRecipe.kitchenObjectSOList, plateKitcheObjectsSOList);

            if (found)
            {
                deliveredRecipeIndex = index;
                break;
            }
        }

        ProcessDeliverRecipeResultServerRpc(success: found, deliveredRecipeIndex);

        return found;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProcessDeliverRecipeResultServerRpc(bool success, int deliveredRecipeIndex) =>
        ProcessDeliverRecipeResultClientRpc(success, deliveredRecipeIndex);

    [ClientRpc]
    private void ProcessDeliverRecipeResultClientRpc(bool success, int deliveredRecipeIndex)
    {
        if (success)
        {
            waitingRecipeSOList.RemoveAt(deliveredRecipeIndex);
            OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
            successfulDeliveriesAmount++;
        }

        NotifyDeliveryResult(success);
    }

    private void NotifyDeliveryResult(bool success)
    {
        if (success)
        {
            OnDeliverySuccess?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnDeliveryFailure?.Invoke(this, EventArgs.Empty);
        }
    }

    private static bool AreEqual<T>(List<T> first, List<T> second)
    {
        var firstNotSecond = first.Except(second).ToList();
        var secondNotFirst = second.Except(first).ToList();

        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;

    public int GetSuccessfulDeliveriesAmount() => successfulDeliveriesAmount;
}
