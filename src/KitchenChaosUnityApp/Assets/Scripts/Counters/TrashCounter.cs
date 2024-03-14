using System;
using System.Collections;
using System.Collections.Generic;
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
        player.GetKitchenObject().DestroySelf();
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
    }

    public override bool CanInteract(Player player) => player.HasKitchenObject;
}
