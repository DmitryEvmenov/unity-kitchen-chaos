using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    private Dictionary<ulong, bool> playersReadyDictionary;

    private void Awake()
    {
        playersReadyDictionary = new Dictionary<ulong, bool>();
        Instance = this;
    }

    public void SetPlayerReady() => SetPlayerReadyServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playersReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        if (CheckIfAllConnectedPlayersReady())
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    private bool CheckIfAllConnectedPlayersReady()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playersReadyDictionary.ContainsKey(clientId) || !playersReadyDictionary[clientId])
                return false;
        }
        return true;
    }
}
