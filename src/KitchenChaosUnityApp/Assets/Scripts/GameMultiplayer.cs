using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private int maxPlayersCount;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;

        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count > maxPlayersCount - 1)
        {
            response.Approved = false;
            response.Reason = $"The game only allows {maxPlayersCount} players maximum";
            return;
        }

        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
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
