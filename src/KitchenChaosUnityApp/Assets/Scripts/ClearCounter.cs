using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    public void Interact(Player player)
    {
        if (HasKitchenObject)
        {
            player.PickUpKitchenObject(GetKitchenObject());
        }
        else
        {
            if (player.HasKitchenObject)
            {
                player.DropKitchenObjectTo(this);
            }
            else
            {
                InstantiateNewKitchenObject();
            }
        }
    }

    private void InstantiateNewKitchenObject() =>
        Instantiate(KitchenObjectSO.prefab, counterTopPoint).GetComponent<KitchenObject>().SetParentKitchenObject(this);

    public Transform KitchenObjectFollowTransform => counterTopPoint;

    public KitchenObject GetKitchenObject() => kitchenObject;

    public void SetKitchenObject(KitchenObject value) => kitchenObject = value;

    public bool HasKitchenObject => kitchenObject != null;

    public void ClearKitchenObject() => kitchenObject = null;
}
