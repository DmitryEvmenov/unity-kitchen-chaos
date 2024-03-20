using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float moveMinReactDelta = .5f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float playerRadius = .7f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;

    private KitchenObject kitchenObject;

    private float MoveDistance => moveSpeed * Time.deltaTime;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerPickUp;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    public static Player LocalInstance { get; private set; }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleMovement();

        if (GameManager.Instance.IsGamePlaying())
        {
            HandleInteractions();
        }
    }

    private void HandleMovement()
    {
        var moveDir = GetCurrentMovementVector();

        IsWalking = moveDir != Vector3.zero;

        if (!IsWalking)
        {
            return;
        }

        if (CanMove(moveDir))
        {
            Move(moveDir);
        }
        else
        {
            var moveDirX = new Vector3(moveDir.x, 0, 0).normalized;

            if (IsLegitMove(moveDir.x) && CanMove(moveDirX))
            {
                Move(moveDirX);
            }
            else
            {
                var moveDirZ = new Vector3(0, 0, moveDir.z).normalized;

                if (IsLegitMove(moveDir.z) && CanMove(moveDirZ))
                {
                    Move(moveDirZ);
                }
            }
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
    }

    private void HandleInteractions()
    {
        var moveDir = GetCurrentMovementVector();

        if (moveDir == Vector3.zero)
        {
            return;
        }

        lastInteractDir = moveDir;

        if (Physics.Raycast(transform.position, lastInteractDir, out var hitInfo, maxDistance: 2f, counterLayerMask))
        {
            if (hitInfo.transform.TryGetComponent(out BaseCounter counter))
            {
                if (counter != selectedCounter && counter.CanInteract(this))
                {
                    SetSelectedCounter(counter);
                }
            }
            else if (selectedCounter != null)
            {
                SetSelectedCounter(null);
            }
        }
        else if (selectedCounter != null)
        {
            SetSelectedCounter(null);
        }
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
    }

    private Vector3 GetCurrentMovementVector()
    {
        var inputVector = GameInput.Instance.GetMovementVectorNormalized();

        return new Vector3(inputVector.x, 0f, inputVector.y);
    }

    private bool CanMove(Vector3 moveDir) => 
        !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, MoveDistance, collisionsLayerMask);

    private bool IsLegitMove(float moveVector) => moveVector > moveMinReactDelta || moveVector < -moveMinReactDelta;

    private void Move(Vector3 moveDir) => transform.position += MoveDistance * moveDir;

    public bool IsWalking { get; private set; }

    public Transform KitchenObjectFollowTransform => kitchenObjectHoldPoint;

    public KitchenObject GetKitchenObject() => kitchenObject;

    public void SetKitchenObject(KitchenObject value)
    {
        kitchenObject = value;

        if (HasKitchenObject)
        {
            OnAnyPlayerPickUp?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool HasKitchenObject => kitchenObject != null;

    public void ClearKitchenObject() => kitchenObject = null;

    public void PutDownKitchenObjectTo(IKitchenObjectParent counter) => GetKitchenObject().SetParentKitchenObject(counter);

    public void PickUpKitchenObject(KitchenObject @object) => @object.SetParentKitchenObject(this);

    public void RefreshSelectedCounter()
    {
        if (!selectedCounter?.CanInteract(this) ?? false)
        {
            SetSelectedCounter(null);
        }
    }
}
