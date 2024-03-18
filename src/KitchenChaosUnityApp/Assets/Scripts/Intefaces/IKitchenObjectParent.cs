using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    Transform KitchenObjectFollowTransform { get; }
    KitchenObject GetKitchenObject();
    void SetKitchenObject(KitchenObject value);
    bool HasKitchenObject { get; }
    void ClearKitchenObject();
    NetworkObject NetworkObject { get; }
}
