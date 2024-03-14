using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions _actions { get; set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;

    private void Awake()
    {
        _actions = new PlayerInputActions();
        _actions.Player.Enable();

        _actions.Player.Interact.performed += Interact_performed;
        _actions.Player.InteractAlternate.performed += InteractAlternate_performed;
        _actions.Player.Pause.performed += Pause_performed;

        Instance = this;
    }

    private void OnDestroy()
    {
        _actions.Player.Interact.performed -= Interact_performed;
        _actions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        _actions.Player.Pause.performed -= Pause_performed;

        _actions.Dispose();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) => 
        OnPauseAction?.Invoke(this, EventArgs.Empty);

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) => 
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) => 
        OnInteractAction?.Invoke(this, EventArgs.Empty);

    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = _actions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
