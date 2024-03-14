using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    private List<RecipeSO> waitingRecipeSOList;

    private float spawnRecipeTimer;


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
        Instance = this;
    }

    private void Update()
    {
        if (waitingRecipeSOList.Count >= waitingRecipeListMax)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMaxSeconds;

            var waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];

            waitingRecipeSOList.Add(waitingRecipeSO);

            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool TryDeliverOrder(PlateKitchenObject plateKitchenObject)
    {
        var found = false;
        var plateKitcheObjectsSOList = plateKitchenObject.GetKitchenObjectSOList();

        foreach (var waitingRecipe in waitingRecipeSOList)
        {
            found = AreEqual(waitingRecipe.kitchenObjectSOList, plateKitcheObjectsSOList);

            if (found)
            {
                waitingRecipeSOList.Remove(waitingRecipe);
                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);

                break;
            }
        }

        NotifyDeliveryResult(found);

        return found;
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
}
