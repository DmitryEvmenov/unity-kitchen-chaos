using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyMoveUpText;
    [SerializeField] private TextMeshProUGUI keyMoveDownText;
    [SerializeField] private TextMeshProUGUI keyMoveLeftText;
    [SerializeField] private TextMeshProUGUI keyMoveRightText;
    [SerializeField] private TextMeshProUGUI keyInteractText;
    [SerializeField] private TextMeshProUGUI keyInteractAlternateText;
    [SerializeField] private TextMeshProUGUI keyPauseText;
    [SerializeField] private TextMeshProUGUI keyGamePadInteractText;
    [SerializeField] private TextMeshProUGUI keyGamePadInteractAlternateText;
    [SerializeField] private TextMeshProUGUI keyGamePadPauseText;

    private void Start()
    {
        UpdateVisuals();

        GameInput.Instance.OnBindingsChanged += GameInput_OnBindingsChanged;
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

        Show();
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.OnGameStateChangedEventArgs e)
    {
        if (e.NewState != GameManager.State.WaitingToStart)
        {
            Hide();
        }
    }

    private void GameInput_OnBindingsChanged(object sender, System.EventArgs e) => UpdateVisuals();

    private void UpdateVisuals()
    {
        keyMoveUpText.text = GetBindingText(GameInput.Binding.MoveUp);
        keyMoveDownText.text = GetBindingText(GameInput.Binding.MoveDown);
        keyMoveLeftText.text = GetBindingText(GameInput.Binding.MoveLeft);
        keyMoveRightText.text = GetBindingText(GameInput.Binding.MoveRight);
        keyInteractText.text = GetBindingText(GameInput.Binding.Interact);
        keyInteractAlternateText.text = GetBindingText(GameInput.Binding.InteractAlternate);
        keyPauseText.text = GetBindingText(GameInput.Binding.Pause);
        keyGamePadInteractText.text = GetBindingText(GameInput.Binding.Gamepad_Interact);
        keyGamePadInteractAlternateText.text = GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
        keyGamePadPauseText.text = GetBindingText(GameInput.Binding.Gamepad_Pause);
    }

    private string GetBindingText(GameInput.Binding binding) => GameInput.Instance.GetBindingText(binding);

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
