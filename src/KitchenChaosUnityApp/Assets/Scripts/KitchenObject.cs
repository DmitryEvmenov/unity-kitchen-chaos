using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent currentKitchenObjectParent;

    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public IKitchenObjectParent GetParentKitchenObject() => currentKitchenObjectParent;

    public void SetParentKitchenObject(IKitchenObjectParent newParent) => 
        SetKitchenObjectParentServerRpc(newParent.NetworkObject);

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference) =>
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out var kitchenObjectParentNetworkObject);
        var newParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

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
        followTransform.SetTargetTransform(currentKitchenObjectParent.KitchenObjectFollowTransform);
    }

    public KitchenObjectSO KitchenObjectSO => kitchenObjectSO;

    public void DestroySelf() => Destroy(gameObject);

    public static void Spawn(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) =>
        GameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject plate)
        {
            plateKitchenObject = plate;
            return true;
        }

        plateKitchenObject = null;
        return false;
    }

    public static void Destroy(KitchenObject kitchenObject) =>
        GameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);

    public void ClearKitchenObjectOnParent() => currentKitchenObjectParent.ClearKitchenObject();
}
