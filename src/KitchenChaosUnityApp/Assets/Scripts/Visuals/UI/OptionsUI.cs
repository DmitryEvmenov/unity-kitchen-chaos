using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance {  get; private set; }

    [SerializeField] private Button soundEffectsButton;

    [SerializeField] private Button musicButton;

    [SerializeField] private Button closeButton;

    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAltButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button gamePadInteractButton;
    [SerializeField] private Button gamePadInteractAltButton;
    [SerializeField] private Button gamePadPauseButton;

    [SerializeField] private TextMeshProUGUI soundEffectsText;

    [SerializeField] private TextMeshProUGUI musicText;

    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAltText;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI gamePadInteractText;
    [SerializeField] private TextMeshProUGUI gamePadInteractAltText;
    [SerializeField] private TextMeshProUGUI gamePadPauseText;

    [SerializeField] private Transform pressToRebindKeyTransform;

    private Action onCloseButtonAction;


    private void Awake()
    {
        soundEffectsButton.onClick.AddListener(() => 
        {
            SoundManager.Instance.ChangeSoundEffectsVolume();
            UpdateSoundEffectsVisual();
        });

        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeMusicVolume();
            UpdateMusicVisual();
        });

        closeButton.onClick.AddListener(Hide);

        moveUpButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveUp));
        moveDownButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveDown));
        moveLeftButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveLeft));
        moveRightButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.MoveRight));
        interactButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Interact));
        interactAltButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.InteractAlternate));
        pauseButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Pause));
        gamePadInteractButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Gamepad_Interact));
        gamePadInteractAltButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Gamepad_InteractAlternate));
        gamePadPauseButton.onClick.AddListener(() => RebindBinding(GameInput.Binding.Gamepad_Pause));

        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        Hide();
        HidePressToRebindKey();
        UpdateSoundEffectsVisual();
        UpdateMusicVisual();
        UpdateButtonsBindingsVisual();
    }

    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e) => Hide();

    public void Show(Action onCloseButtonAction = null)
    {
        soundEffectsButton.Select();
        gameObject.SetActive(true);

        this.onCloseButtonAction = onCloseButtonAction;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onCloseButtonAction?.Invoke();
    }

    private void UpdateSoundEffectsVisual() => soundEffectsText.text = GetTextVisual("Sound Effects", SoundManager.Instance.GetSoundEffectsVolume());

    private void UpdateMusicVisual() => musicText.text = GetTextVisual("Music", MusicManager.Instance.GetMusicVolume());

    private string GetTextVisual(string prefix, float volume) => $"{prefix}: {Mathf.Round(volume * 10f)}";

    private void UpdateButtonsBindingsVisual()
    {
        moveUpText.text = GetBindingText(GameInput.Binding.MoveUp);
        moveDownText.text = GetBindingText(GameInput.Binding.MoveDown);
        moveLeftText.text = GetBindingText(GameInput.Binding.MoveLeft);
        moveRightText.text = GetBindingText(GameInput.Binding.MoveRight);
        interactText.text = GetBindingText(GameInput.Binding.Interact);
        interactAltText.text = GetBindingText(GameInput.Binding.InteractAlternate);
        pauseText.text = GetBindingText(GameInput.Binding.Pause);
        gamePadInteractText.text = GetBindingText(GameInput.Binding.Gamepad_Interact);
        gamePadInteractAltText.text = GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
        gamePadPauseText.text = GetBindingText(GameInput.Binding.Gamepad_Pause);
    }

    private string GetBindingText(GameInput.Binding binding) => GameInput.Instance.GetBindingText(binding);

    private void ShowPressToRebindKey() => pressToRebindKeyTransform.gameObject.SetActive(true);

    private void HidePressToRebindKey() => pressToRebindKeyTransform.gameObject.SetActive(false);

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.Rebind(binding, () =>
        {
            HidePressToRebindKey();
            UpdateButtonsBindingsVisual();
        });
    }
}
