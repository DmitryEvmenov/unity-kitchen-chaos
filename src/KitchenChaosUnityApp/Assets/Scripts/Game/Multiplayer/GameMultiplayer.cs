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
    [SerializeField] private List<Color> playerColorList;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    private NetworkList<PlayerData> playerDataNetworkList;

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Host_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Host_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Host_OnClientDisconnectCallback(ulong clientId)
    {
        playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));
    }

    private void NetworkManager_Host_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { ClientId = clientId, ColorId = GetFirstAvailableColorId() });
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

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
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

    public bool IsPlayerIndexConnected(int playerIndex) => playerIndex < playerDataNetworkList.Count;

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex) => playerDataNetworkList[playerIndex];

    public PlayerData GetPlayerDataFromClientId(ulong clientId) => playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)];

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].ClientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public PlayerData GetLocalPlayerData() => GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public void ChangeLocalPlayerColor(int colorId) => ChangePlayerColorServerRpc(colorId);

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            return;
        }

        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = playerDataNetworkList[playerDataIndex];

        playerData.ColorId = colorId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.ColorId == colorId)
            {
                return false;
            }
        }

        return true;
    }

    private int GetFirstAvailableColorId()
    {
        for (var i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }

        return -1;
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    public int GetMaxPlayersCount() => maxPlayersCount;
}
