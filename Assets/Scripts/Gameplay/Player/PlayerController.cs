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
        private CinemachineCamera _camera;

        private float xRotation = 0f;

        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float mouseSensitivity = 100f;

        // Acceleration

        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;

        private Vector3 currentVelocity = Vector3.zero;
        public Vector3 CurrentVelocity => currentVelocity; // For the shake script.

        // Jumping

        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;

        private bool isGrounded;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                // Disable camera for non-local players
                _rb = GetComponent<Rigidbody>();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                // Freeze rotation so Rigidbody doesn't tip over
                _rb.freezeRotation = true;

                var camComponent = GetComponentInChildren<CinemachineCamera>();
                if (camComponent != null)
                    camComponent.enabled = false;

                return;
            }
        }

        private void Update()
        {
            if (!isLocalPlayer)
                return;

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

            // Blend toward target velocity for smooth acceleration/deceleration
            if (inputDir.magnitude > 0)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
            }

            _rb.MovePosition(_rb.position + currentVelocity * Time.deltaTime);

            Debug.Log("Current Velocity: " + currentVelocity.magnitude.ToString("F2"));
        }

        private void HandleJump()
        {
            // Ground check using a small sphere at the player's feet
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                Debug.Log("Jumped!");
            }
        }


        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotate player horizontally
            transform.Rotate(Vector3.up * mouseX);

            // Rotate camera vertically
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, 0f, 45f);
            _camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
