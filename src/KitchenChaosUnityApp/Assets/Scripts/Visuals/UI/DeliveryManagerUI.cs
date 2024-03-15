using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;
    [SerializeField] private TextMeshProUGUI titleText;

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;

        UpdateVisual();
        titleText.gameObject.SetActive(false);
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e) => UpdateVisual();
    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e) => UpdateVisual();

    private void UpdateVisual()
    {
        foreach (Transform child in container)
        {
            if (child == recipeTemplate)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        var recipeWaitingList = DeliveryManager.Instance.GetWaitingRecipeSOList();

        foreach (var recipeSO in recipeWaitingList)
        {
            var recipeTransform = Instantiate(recipeTemplate, container);
            recipeTransform.gameObject.SetActive(true);
            recipeTransform.GetComponent<DeliveryManagerSingleUI>().SetRecipeSO(recipeSO);
        }

        if (recipeWaitingList.Any())
        {
            titleText.gameObject.SetActive(true);
        }
    }
}
