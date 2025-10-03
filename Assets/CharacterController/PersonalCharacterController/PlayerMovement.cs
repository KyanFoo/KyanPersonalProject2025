using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Reference")]
        public CinemachineVirtualCamera virtualCamera;
        public Transform playerBody;
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private CapsuleCollider playerCollider;
        public Transform orientation;

        [Header("Input Settings")]
        public float sensX = 150f;
        public float sensY = 150f;

        private float mouseX;
        private float mouseY;
        private float verticalInput;
        private float horizontalInput;

        private float xRotation;
        private float yRotation;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;
        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;
        public bool isGrounded;

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        public void Update()
        {
            InputManagement();
            CameraMovement();
        }

        private void FixedUpdate()
        {
            isGrounded = CheckGrounded();
        }

        private void CameraMovement()
        {
            mouseX *= sensX * Time.deltaTime;
            mouseY *= sensY * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90, 90);

            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private Vector3 FeetPosition()
        {
            Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            Vector3 feetPosition = playerRigidbody.position + sphereOffset;
            return feetPosition;
        }

        private bool CheckGrounded()
        {
            Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;
            float checkDistance = groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET;
            return Physics.SphereCast(checkOrigin, playerCollider.radius * transform.localScale.x, -transform.up, out RaycastHit _, checkDistance, groundMask);
        }

        private void InputManagement()
        {
            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");

            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");
        }
    }
}
