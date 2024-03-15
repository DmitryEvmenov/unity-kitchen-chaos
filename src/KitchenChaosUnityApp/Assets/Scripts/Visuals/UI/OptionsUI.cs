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

    [SerializeField] private TextMeshProUGUI soundEffectsText;

    [SerializeField] private TextMeshProUGUI musicText;


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

        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        Hide();
        UpdateSoundEffectsVisual();
        UpdateMusicVisual();
    }

    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e) => Hide();

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    private void UpdateSoundEffectsVisual() => soundEffectsText.text = GetTextVisual("Sound Effects", SoundManager.Instance.GetSoundEffectsVolume());

    private void UpdateMusicVisual() => musicText.text = GetTextVisual("Music", MusicManager.Instance.GetMusicVolume());

    private string GetTextVisual(string prefix, float volume) => $"{prefix}: {Mathf.Round(volume * 10f)}";
}
