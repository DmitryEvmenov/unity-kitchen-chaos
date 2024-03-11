using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent currentKitchenObjectParent;

    public IKitchenObjectParent GetParentKitchenObject() => currentKitchenObjectParent;

    public void SetParentKitchenObject(IKitchenObjectParent newParent)
    {
        var doesNewParentHaveObjectToSwap = newParent.HasKitchenObject && (currentKitchenObjectParent?.HasKitchenObject ?? false);
        if (doesNewParentHaveObjectToSwap)
        {
            SwapParents(this, newParent.GetKitchenObject());
            return;
        }

        currentKitchenObjectParent?.ClearKitchenObject();
        currentKitchenObjectParent = newParent;
        currentKitchenObjectParent.SetKitchenObject(this);

        FixUpTransformPosition();
    }

    private static void SwapParents(KitchenObject first, KitchenObject second)
    {
        var parentFirst = first.GetParentKitchenObject();
        var parentSecond = second.GetParentKitchenObject();

        parentFirst.ClearKitchenObject();
        parentSecond.ClearKitchenObject();

        first.currentKitchenObjectParent = parentSecond;
        second.currentKitchenObjectParent = parentFirst;

        parentFirst.SetKitchenObject(second);
        parentSecond.SetKitchenObject(first);

        first.FixUpTransformPosition();
        second.FixUpTransformPosition();
    }

    private void FixUpTransformPosition()
    {
        transform.parent = currentKitchenObjectParent.KitchenObjectFollowTransform;
        transform.localPosition = Vector3.zero;
    }

    public KitchenObjectSO KitchenObjectSO => kitchenObjectSO;

    public void DestroySelf()
    {
        currentKitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }

    public static KitchenObject Spawn(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        var spawnedObject = Instantiate(kitchenObjectSO.prefab).GetComponent<KitchenObject>();
        spawnedObject.SetParentKitchenObject(kitchenObjectParent);

        return spawnedObject;
    }
}
