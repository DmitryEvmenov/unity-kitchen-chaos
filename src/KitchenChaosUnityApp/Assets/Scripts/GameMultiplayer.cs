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
        SpawnKitchenObjectServerRpc(GetKitcheObjectSOIndex(kitchenObjectSO), kitchenObjectParent.NetworkObject);

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        var kitchenObjectSO = GetKitchenObjectSOByIndex(kitchenObjectSOIndex);

        var transform = Instantiate(kitchenObjectSO.prefab);
        var networkObject = transform.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);

        var kitchenObject = transform.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectReference.TryGet(out var kitchenObjectParentNetworkObject);
        var parent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetParentKitchenObject(parent);
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject) => DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        var kitchenObject = TryGetKitchenObjectComponent(kitchenObjectNetworkObjectReference);

        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);

        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        var kitchenObject = TryGetKitchenObjectComponent(kitchenObjectNetworkObjectReference);

        kitchenObject.ClearKitchenObjectOnParent();
    }

    private KitchenObject TryGetKitchenObjectComponent(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out var kitchenObjectNetworkObject);
        return kitchenObjectNetworkObject.GetComponent<KitchenObject>();
    }

    public int GetKitcheObjectSOIndex(KitchenObjectSO kitchenObjectSO) => kitchenObjectListSO.KitchenObjectSOList.IndexOf(kitchenObjectSO);

    public KitchenObjectSO GetKitchenObjectSOByIndex(int index) => kitchenObjectListSO.KitchenObjectSOList[index];
}
