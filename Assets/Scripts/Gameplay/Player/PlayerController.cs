using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Steamworks;

namespace SteamLobby
{
    public class PlayerController : NetworkBehaviour
    {
        private Rigidbody _rb;
        private Camera _camera;

        private float xRotation = 0f;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float mouseSensitivity = 100f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<Camera>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (!isLocalPlayer)
            {
                _camera.enabled = false;
            }

            // Freeze rotation so Rigidbody doesn't tip over
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            if (!isLocalPlayer)
                return;

            HandleMovement();
            HandleMouseLook();
        }

        private void HandleMovement()
        {
            Vector3 moveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) moveDir += transform.forward;
            if (Input.GetKey(KeyCode.S)) moveDir -= transform.forward;
            if (Input.GetKey(KeyCode.A)) moveDir -= transform.right;
            if (Input.GetKey(KeyCode.D)) moveDir += transform.right;

            float currentSpeed = moveSpeed;

            Vector3 newPosition = _rb.position + moveDir.normalized * currentSpeed * Time.deltaTime;
            _rb.MovePosition(newPosition);

            Debug.Log("Greitis: " + currentSpeed);
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
