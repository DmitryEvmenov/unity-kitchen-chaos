using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrashed;

    public static new void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }

    protected override void OnInteract(Player player)
    {
        KitchenObject.Destroy(player.GetKitchenObject());
        InteractLogicServerRpc();
    }

    public override bool CanInteract(Player player) => player.HasKitchenObject;

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
    }
}
