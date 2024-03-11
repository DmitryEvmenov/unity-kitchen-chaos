using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions _actions { get; set; }

    public event EventHandler OnInteractAction;

    private void Awake()
    {
        _actions = new PlayerInputActions();
        _actions.Player.Enable();

        _actions.Player.Interact.performed += Interact_performed;
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = _actions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
