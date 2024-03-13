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

            var waitingRecipeSO = recipeListSO.recipeSOList[Random.Range(0, recipeListSO.recipeSOList.Count)];

            waitingRecipeSOList.Add(waitingRecipeSO);

            Debug.Log(waitingRecipeSO.name);
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
                break;
            }
        }

        if (found)
        {
            Debug.Log("Correct order delivered!");
        }
        else
        {
            Debug.Log("No matching recipe order found :(");
        }

        return found;
    }

    private static bool AreEqual<T>(List<T> first, List<T> second)
    {
        var firstNotSecond = first.Except(second).ToList();
        var secondNotFirst = second.Except(first).ToList();

        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }
}
