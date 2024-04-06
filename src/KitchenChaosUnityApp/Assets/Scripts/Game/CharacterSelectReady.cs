using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    private Dictionary<ulong, bool> playersReadyDictionary;

    public event EventHandler OnReadyChanged;

    private void Awake()
    {
        playersReadyDictionary = new Dictionary<ulong, bool>();
        Instance = this;
    }

    public void SetPlayerReady() => SetPlayerReadyServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        playersReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        if (CheckIfAllConnectedPlayersReady())
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playersReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
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

    public bool IsPlayerReady(ulong clientId) => playersReadyDictionary.ContainsKey(clientId) && playersReadyDictionary[clientId];
}
