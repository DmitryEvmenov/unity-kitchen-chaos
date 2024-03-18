using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    public static event EventHandler OnAnyObjectPlaced;

    public static void ResetStaticData()
    {
        OnAnyObjectPlaced = null;
    }

    public virtual void Interact(Player player) => TryInteract(player, OnInteract);

    protected abstract void OnInteract(Player player);

    public virtual void InteractAlternate(Player player) => TryInteract(player, OnInteractAlternate);

    protected virtual void OnInteractAlternate(Player player) 
    {
    }

    private void TryInteract(Player player, Action<Player> interactAction)
    {
        if (CanInteract(player))
        {
            interactAction(player);

            player.RefreshSelectedCounter();
        }
    }

    public Transform KitchenObjectFollowTransform => counterTopPoint;

    public KitchenObject GetKitchenObject() => kitchenObject;

    public void SetKitchenObject(KitchenObject value)
    {
        kitchenObject = value;

        if (HasKitchenObject)
        {
            OnAnyObjectPlaced?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool HasKitchenObject => kitchenObject != null;

    public void ClearKitchenObject() => kitchenObject = null;

    public virtual bool CanInteract(Player player) => true;

    public NetworkObject NetworkObject => null;
}
