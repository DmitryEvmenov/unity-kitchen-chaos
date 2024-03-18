using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) =>
        SpawnKitchenObjectServerRpc(kitchenObjectListSO.KitchenObjectSOList.IndexOf(kitchenObjectSO), kitchenObjectParent.NetworkObject);

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        var kitchenObjectSO = kitchenObjectListSO.KitchenObjectSOList[kitchenObjectSOIndex];

        var transform = Instantiate(kitchenObjectSO.prefab);
        var networkObject = transform.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);

        var kitchenObject = transform.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectReference.TryGet(out var kitchenObjectParentNetworkObject);
        var parent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetParentKitchenObject(parent);
    }
}
