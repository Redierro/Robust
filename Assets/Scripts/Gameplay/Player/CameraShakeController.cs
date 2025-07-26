using UnityEngine;
using Unity.Cinemachine;
using Mirror;
using Steamworks;
using SteamLobby;

public class CameraShakeController : NetworkBehaviour
{
    [Header("Shake values")]
    [SerializeField] private CinemachineCamera virtualCam;
    [SerializeField] private float walkAmplitude = 1f;
    [SerializeField] private float walkFrequency = 0.2f;
    [SerializeField] private float runAmplitude = 1.7f;
    [SerializeField] private float runFrequency = 0.4f;
    [SerializeField] private float shakeDamping = 5f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float targetAmplitude = 0f;
    private float currentAmplitude = 0f;
    private float targetFrequency = 0f;
    private float currentFrequency = 0f;
    private bool isMoving = false;
    private bool isRunning = false;

    [Header("ETC")]
    public IngameUI igUI;
    private PlayerController playerController;

    void Start()
    {
        noise = virtualCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (!isLocalPlayer || ChatManager.Instance == null || playerController == null || noise == null) return;

        if (ChatManager.Instance.chatRaised || igUI.escapeRaised)
        {
            isMoving = false;
            isRunning = false;
        }
        else
        {
            isMoving = playerController.currentVelocity.magnitude > 0.1f;
            isRunning = playerController.targetSpeed == playerController.runSpeed &&
                        playerController.playerStats.CanRun(); // Assumes playerStats has CanRun()
        }

        // Set target shake
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

        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * shakeDamping);
        currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, Time.deltaTime * shakeDamping);

        noise.AmplitudeGain = currentAmplitude;
        noise.FrequencyGain = currentFrequency;
    }
}
