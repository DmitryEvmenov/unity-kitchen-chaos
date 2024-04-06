using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Color successColor;
    [SerializeField] private Color failColor;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failSprite;

    private Animator animator;

    private const string DELIVERY_RESULT_POPUP = "Popup";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        DeliveryManager.Instance.OnDeliverySuccess += DeliveryManager_OnDeliverySuccess;
        DeliveryManager.Instance.OnDeliveryFailure += DeliveryManager_OnDeliveryFailure;

        gameObject.SetActive(false);
    }

    private void DeliveryManager_OnDeliveryFailure(object sender, System.EventArgs e) =>
        InitDeliveryResultUI("DELIVERY\nFAILURE", failColor, failSprite);

    private void DeliveryManager_OnDeliverySuccess(object sender, System.EventArgs e) =>
        InitDeliveryResultUI("DELIVERY\nSUCCESS", successColor, successSprite);

    private void InitDeliveryResultUI(string messageText, Color backgroundColor, Sprite iconSprite)
    {
        this.messageText.text = messageText;
        backgroundImage.color = backgroundColor;
        iconImage.sprite = iconSprite;

        gameObject.SetActive(true);

        animator.SetTrigger(DELIVERY_RESULT_POPUP);
    }
}
