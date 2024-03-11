using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float playerRadius = .7f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private KitchenObject kitchenObject;

    private float MoveDistance => moveSpeed * Time.deltaTime;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    public static Player Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) 
        {
            Debug.Log("Another player instance found");
        }

        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleMovement()
    {
        var moveDir = GetCurrentMovementVector();

        if (CanMove(moveDir))
        {
            Move(moveDir);
        }
        else
        {
            var moveDirX = new Vector3(moveDir.x, 0, 0).normalized;

            if (CanMove(moveDirX))
            {
                Move(moveDirX);
            }
            else
            {
                var moveDirZ = new Vector3(0, 0, moveDir.z).normalized;

                if (CanMove(moveDirZ))
                {
                    Move(moveDirZ);
                }
            }
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);

        IsWalking = moveDir != Vector3.zero;
    }

    private void HandleInteractions()
    {
        var moveDir = GetCurrentMovementVector();

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        if (Physics.Raycast(transform.position, lastInteractDir, out var hitInfo, maxDistance: 2f, counterLayerMask))
        {
            if (hitInfo.transform.TryGetComponent(out BaseCounter counter))
            {
                if (counter != selectedCounter)
                {
                    SetSelectedCounter(counter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
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
        var inputVector = gameInput.GetMovementVectorNormalized();

        return new Vector3(inputVector.x, 0f, inputVector.y);
    }

    private bool CanMove(Vector3 moveDir) => 
        !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, MoveDistance);

    private void Move(Vector3 moveDir) => transform.position += MoveDistance * moveDir;

    public bool IsWalking { get; private set; }

    public Transform KitchenObjectFollowTransform => kitchenObjectHoldPoint;

    public KitchenObject GetKitchenObject() => kitchenObject;

    public void SetKitchenObject(KitchenObject value) => kitchenObject = value;

    public bool HasKitchenObject => kitchenObject != null;

    public void ClearKitchenObject() => kitchenObject = null;

    public void DropKitchenObjectTo(IKitchenObjectParent counter) => GetKitchenObject().SetParentKitchenObject(counter);
    public void PickUpKitchenObject(KitchenObject @object) => @object.SetParentKitchenObject(this);
}
