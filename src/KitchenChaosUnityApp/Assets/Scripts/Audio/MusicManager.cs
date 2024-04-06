using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private float defaultMusicVolume;

    [SerializeField] private float volumeIncreaseIncrement;

    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;

        defaultMusicVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, defaultMusicVolume);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        RefreshMusicVolume();
    }

    public void ChangeMusicVolume() => ToggleIncreaseVolume(ref defaultMusicVolume);

    private void ToggleIncreaseVolume(ref float volume)
    {
        volume += volumeIncreaseIncrement;

        if (volume > 1)
        {
            volume = 0;
        }

        RefreshMusicVolume();

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }

    private void RefreshMusicVolume() => audioSource.volume = defaultMusicVolume;

    public float GetMusicVolume() => defaultMusicVolume;
}
