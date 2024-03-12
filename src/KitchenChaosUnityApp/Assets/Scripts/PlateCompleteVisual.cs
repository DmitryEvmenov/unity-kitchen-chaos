using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitcheObject;
    [SerializeField] private List<KitcheObjectSO_GameObject> kitcheObjectSO_GameObjects;

    [Serializable]
    public struct KitcheObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    private void Start()
    {
        plateKitcheObject.OnIngredientAdded += PlateKitcheObject_OnIngredientAdded;
    }

    private void PlateKitcheObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        var gameObjectToActivate = kitcheObjectSO_GameObjects.First(x => x.kitchenObjectSO == e.KitchenObjectSO);
        gameObjectToActivate.gameObject.SetActive(true);
    }
}
