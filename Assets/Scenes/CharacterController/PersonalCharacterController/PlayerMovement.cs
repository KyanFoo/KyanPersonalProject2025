using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

        [Header("Reference")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private CapsuleCollider playerCollider;

        [Header("Input Settings")]
        [SerializeField] private float sensX; 
        [SerializeField] private float sensY;

        private float mouseX;
        private float mouseY;
        private float verticalInput;
        private float horizontalInput;

        private float xRotation;
        private float yRotation;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;

        public bool isGrounded;

        [Header("DebugDraw Settings")]
        public bool debug = false;

        private void Update()
        {
            InputManagement();
            CameraMovement();
        }

        private void FixedUpdate()
        {
            isGrounded = IsGrounded();
        }

        private void CameraMovement()
        {
            mouseX *= sensX * Time.deltaTime;
            mouseY *= sensY * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90, 90);

            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private Vector3 FeetPosition()
        {
            Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            //-----Explaination-----//
            // (height / 2 - radius) gives you the distance from center to bottom of the flat capsule base, before the rounded part.
            // (Multiply by -1 * transform.up) pushes the offset downward in world space(i.e., towards the feet), regardless of player rotation.
            // [transform.up = Depends direction depends on the value. [+1 -> Up], [0 -> Centre], [-1 -> Down]

            Vector3 feetPosition = playerRigidbody.position + sphereOffset;
            // [playerRigidbody.position = Centre of playerRigidbody]

            return feetPosition;
        }

        private bool IsGrounded()
        {
            // Calculate the starting point of the SphereCast just above the feet to avoid clipping into the floor
            Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;

            // Define the length of the SphereCast including the offset
            float checkDistance = groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET;

            // True if grounded; otherwise, false
            return Physics.SphereCast(checkOrigin, playerCollider.radius * transform.localScale.x, -transform.up, out RaycastHit _, checkDistance, groundMask);
        }

        private void InputManagement()
        {
            // Gather inputs from the WASD keys for movement.
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            //Gather input from the mouse for camera movement.
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        private void OnDrawGizmos() // --- Debug gizmos for visualizing ground check ---
        {
            if (debug)
            {
                // Draw a WireSphere to visualize a FeetPosition Collidier for Player Prefab for Ground & Slope Check.
                Vector3 feetOffset = -1 * groundCheckDistance * transform.up;
                Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
            }
        }
    }
}
