using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [SerializeField] private float defaultVolume;

    private void Start()
    {
        DeliveryManager.Instance.OnDeliverySuccess += DeliveryManager_OnDeliverySuccess;
        DeliveryManager.Instance.OnDeliveryFailure += DeliveryManager_OnDeliveryFailure;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.Instance.OnPickUp += Player_OnPickUp;
        BaseCounter.OnAnyObjectPlaced += Counter_OnAnyObjectPlaced;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.trash, (sender as TrashCounter).transform.position);

    private void Counter_OnAnyObjectPlaced(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.objectDrop, (sender as BaseCounter).transform.position);

    private void Player_OnPickUp(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.chop, (sender as CuttingCounter).transform.position);

    private void DeliveryManager_OnDeliveryFailure(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.deliveryFail, DeliveryCounter.Instance.transform.position);

    private void DeliveryManager_OnDeliverySuccess(object sender, System.EventArgs e) =>
        PlayRandomFromArraySound(audioClipRefsSO.deliverySuccess, DeliveryCounter.Instance.transform.position);

    private void PlaySound(AudioClip audioClip, Vector3? position = null, float? volume = null) => 
        AudioSource.PlayClipAtPoint(audioClip, position ?? Camera.main.transform.position, volume ?? defaultVolume);

    private void PlayRandomFromArraySound(AudioClip[] audioClipsArray, Vector3? position = null, float? volume = null) =>
        PlaySound(audioClipsArray[Random.Range(0, audioClipsArray.Length)], position, volume);

    public void PlayFootstepsSound(Vector3 position) =>
        PlayRandomFromArraySound(audioClipRefsSO.footstep, position);
}
