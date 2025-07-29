using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Steamworks;
using Unity.Cinemachine;

namespace SteamLobby
{
    public class PlayerController : NetworkBehaviour
    {
        private Rigidbody _rb;

        [Header("Camera Rig")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private Transform cinemachineCamera;
        private float xRotation = 0f;
        [SerializeField] private float mouseSensitivity = 100f;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] public float runSpeed = 10f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        public float targetSpeed;
        private bool wasRunningOnJump = false;
        public bool isMovementLocked = false;
        private Vector3 momentumVelocity = Vector3.zero;
        public Vector3 currentVelocity = Vector3.zero;
        public Vector3 CurrentVelocity => currentVelocity;
        public bool IsActuallyMoving => currentVelocity.magnitude > 0.1f;
        public bool IsActuallyRunning => targetSpeed == runSpeed && playerStats.CanRun();

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;
        private bool isGrounded;
        private bool wasFalling = false;
        private float previousYVelocity = 0f;

        [Header("PlayerStats")]
        [SerializeField] public PlayerStats playerStats;

        [Header("ETC")]
        public IngameUI igUI;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                var unityCam = GetComponentInChildren<Camera>();
                var cineCam = GetComponentInChildren<CinemachineCamera>();
                var brain = unityCam?.GetComponent<CinemachineBrain>();
                playerStats = GetComponent<PlayerStats>();

                if (unityCam) unityCam.enabled = false;
                if (brain) brain.enabled = false;
                if (cineCam) cineCam.enabled = false;

                return;
            }

            _rb = GetComponent<Rigidbody>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            if (!isLocalPlayer || ChatManager.Instance == null) return;

            if (ChatManager.Instance.chatRaised || igUI.escapeRaised || isMovementLocked) return; // Chat/option menu/inventory is open, block control

            HandleMovement();
            HandleMouseLook();
            HandleJump();
        }

        private void HandleMovement()
        {
            Vector3 inputDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) inputDir += transform.forward;
            if (Input.GetKey(KeyCode.S)) inputDir -= transform.forward;
            if (Input.GetKey(KeyCode.A)) inputDir -= transform.right;
            if (Input.GetKey(KeyCode.D)) inputDir += transform.right;

            inputDir.Normalize();

            if (isGrounded)
            {
                targetSpeed = (Input.GetKey(KeyCode.LeftShift) && playerStats.CanRun()) ? runSpeed : walkSpeed;
                if (targetSpeed == runSpeed)
                    playerStats.ConsumeEnergyForRun();


                Vector3 targetVelocity = inputDir * targetSpeed;

                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity,
                    (inputDir.magnitude > 0 ? acceleration : deceleration) * Time.deltaTime);

                momentumVelocity = currentVelocity; // Save grounded velocity
            }
            else
            {
                // Lock in the momentum at the moment of jumping
                currentVelocity = momentumVelocity;
            }

            _rb.MovePosition(_rb.position + currentVelocity * Time.deltaTime);
        }
        private void HandleJump()
        {
            previousYVelocity = _rb.linearVelocity.y;

            bool wasGroundedLastFrame = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && playerStats.CanJump())
            {
                playerStats.ConsumeEnergyForJump();
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                wasRunningOnJump = Input.GetKey(KeyCode.LeftShift);
                momentumVelocity = currentVelocity;
            }

            // Don't reset momentum immediately when grounded
            if (wasGroundedLastFrame == false && isGrounded)
            {
                wasRunningOnJump = false;
            }

            // Detect landing
            if (!wasGroundedLastFrame && isGrounded && previousYVelocity < -0.1f)
            {
                float impactSpeed = Mathf.Abs(previousYVelocity);
                playerStats.FallDamage(impactSpeed);
                Debug.Log("Impact speed - " + impactSpeed);
            }

        }
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotate the player horizontally (left/right)
            transform.Rotate(Vector3.up * mouseX);

            // Rotate the camera vertically (up/down)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -45f, 45f);

            cinemachineCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
