using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        DestroyIfExists(NetworkManager.Singleton);
        DestroyIfExists(GameMultiplayer.Instance);
        DestroyIfExists(GameLobby.Instance);
    }

    private void DestroyIfExists(Behaviour singletonInstance)
    {
        if (singletonInstance != null)
        {
            Destroy(singletonInstance.gameObject);
        }
    }
}
