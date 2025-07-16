using UnityEngine;
using Unity.Cinemachine;
using Mirror;
using Steamworks;

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

    void Start()
    {
        noise = virtualCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (noise == null) return;

        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                        Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

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
