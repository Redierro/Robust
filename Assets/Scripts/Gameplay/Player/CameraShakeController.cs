using UnityEngine;
using Unity.Cinemachine;
using Mirror;
using Steamworks;
using SteamLobby;
public class CameraShakeController : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera virtualCam;
    [SerializeField] private float walkAmplitude = 1f;
    [SerializeField] private float walkFrequency = 0.2f;
    [SerializeField] private float runAmplitude = 1.7f;
    [SerializeField] private float runFrequency = 0.4f;
    [SerializeField] private float shakeDamping = 5f; // fade speed

    private CinemachineBasicMultiChannelPerlin noise;
    private float targetAmplitude = 0f;
    private float currentAmplitude = 0f;
    private float targetFrequency = 0f;
    private float currentFrequency = 0f;

    bool isMoving = false;
    bool isRunning = false;

    void Start()
    {
        noise = virtualCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (!isLocalPlayer || ChatManager.Instance == null) return;

        if (ChatManager.Instance.chatRaised || IngameUI.Instance.escapeRaised)
        {
            isMoving = false;
            isRunning = false;
        } // Chat is open, block control
        else
        {
            if (noise == null) return;

            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))) { isMoving = false; isRunning = false; }
            else if (this.GetComponentInParent<PlayerController>().targetSpeed == 4) { isMoving = true; isRunning = false; }
            else if (this.GetComponentInParent<PlayerController>().targetSpeed == 8) { isRunning = true; isMoving = true; }
        }

        // Set target shake based on movement state
        if (isMoving)
        {
            targetAmplitude = isRunning ? runAmplitude : walkAmplitude;
            targetFrequency = isRunning ? runFrequency : walkFrequency;
        }
        else
        {
            targetAmplitude = 0f;
            targetFrequency = 0f;
        }

        // Smoothly fade shake in/out
        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * shakeDamping);
        currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, Time.deltaTime * shakeDamping);

        noise.AmplitudeGain = currentAmplitude;
        noise.FrequencyGain = currentFrequency;
    }
}
