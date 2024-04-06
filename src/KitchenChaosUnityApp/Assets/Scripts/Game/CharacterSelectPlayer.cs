using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int index;

    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickPlayerButton;

    private void Awake()
    {
        kickPlayerButton.onClick.AddListener(() =>
        {
            var playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(index);
            GameMultiplayer.Instance.KickPlayer(playerData.ClientId);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        kickPlayerButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(index))
        {
            Show();

            var playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(index);
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));

            playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.ColorId));
        }
        else
        {
            Hide();
        }
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
