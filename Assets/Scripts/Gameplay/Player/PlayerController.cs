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
        [SerializeField] private Transform cameraPivot; // Set this to CameraPivot in inspector
        [SerializeField] private float rotationSmoothSpeed = 5f;
        private float xRotation = 0f;

        [Header("Camera")]
        [SerializeField] private float mouseSensitivity = 100f;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        private Vector3 currentVelocity = Vector3.zero;
        public Vector3 CurrentVelocity => currentVelocity;

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;
        private bool isGrounded;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                var unityCam = GetComponentInChildren<Camera>();
                var cineCam = GetComponentInChildren<CinemachineCamera>();
                var brain = unityCam?.GetComponent<CinemachineBrain>();

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
            if (!isLocalPlayer) return;

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

            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 targetVelocity = inputDir * targetSpeed;

            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity,
                (inputDir.magnitude > 0 ? acceleration : deceleration) * Time.deltaTime);

            _rb.MovePosition(_rb.position + currentVelocity * Time.deltaTime);
        }

        private void HandleJump()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Horizontal rotation of player
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation of camera pivot (with smoothing)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, 0f, 45f);

            float currentX = Mathf.LerpAngle(cameraPivot.localEulerAngles.x, xRotation, Time.deltaTime * rotationSmoothSpeed);
            cameraPivot.localRotation = Quaternion.Euler(currentX, 0f, 0f);
        }
    }
}
