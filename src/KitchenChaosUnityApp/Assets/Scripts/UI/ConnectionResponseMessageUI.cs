using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        messageText.text = !string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason)
            ? NetworkManager.Singleton.DisconnectReason
            : "Failed to connect.";

        Show();
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}
