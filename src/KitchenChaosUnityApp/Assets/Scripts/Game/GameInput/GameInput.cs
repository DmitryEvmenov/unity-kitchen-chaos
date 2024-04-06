using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    private PlayerInputActions _actions { get; set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingsChanged;

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause
    }

    private void Awake()
    {
        _actions = new PlayerInputActions();
        TryLoadExistingPlayerPrefs();
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

    public string GetBindingText(Binding binding)
    {
        var (action, bindingIndex) = GetBindingInputAction(binding);

        return action.bindings[bindingIndex].ToDisplayString();
    }

    private (InputAction action, int bindingIndex) GetBindingInputAction(Binding binding) =>
        binding switch
        {
            Binding.MoveUp => (_actions.Player.Move, 1),
            Binding.MoveDown => (_actions.Player.Move, 2),
            Binding.MoveLeft => (_actions.Player.Move, 3),
            Binding.MoveRight => (_actions.Player.Move, 4),
            Binding.Interact => (_actions.Player.Interact, 0),
            Binding.InteractAlternate => (_actions.Player.InteractAlternate, 0),
            Binding.Pause => (_actions.Player.Pause, 0),
            Binding.Gamepad_Interact => (_actions.Player.Interact, 1),
            Binding.Gamepad_InteractAlternate => (_actions.Player.InteractAlternate, 1),
            Binding.Gamepad_Pause => (_actions.Player.Pause, 1),
            _ => throw new ArgumentOutOfRangeException("Unsupported binding detected")
        };

    public void Rebind(Binding binding, Action onRebindingComplete = null)
    {
        _actions.Player.Disable();

        var (action, bindingIndex) = GetBindingInputAction(binding);

        action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                _actions.Player.Enable();
                onRebindingComplete?.Invoke();

                var bindingsAsJson = _actions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, bindingsAsJson);
                PlayerPrefs.Save();

                OnBindingsChanged?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }

    private void TryLoadExistingPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            _actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
    }
}
