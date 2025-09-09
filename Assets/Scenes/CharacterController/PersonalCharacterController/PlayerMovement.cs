using System.Collections;
using System.Collections.Generic;
using System.Data;
using Cinemachine;
using UnityEngine;
using static KyanPersonalProject2025.CharacterController1.PlayerMovement;

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
        [SerializeField] private Transform orientation;

        [Header("Input Settings")]
        [SerializeField] private float sensX; 
        [SerializeField] private float sensY;

        private float mouseX;
        private float mouseY;
        private float verticalInput;
        private float horizontalInput;

        private float xRotation;
        private float yRotation;

        private Vector3 finalDir;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float airControlMultiplier = 0.4f;
        [SerializeField] private float velocityMagnitude;

        private float moveSpeed;
        private float minVelocity = 0.1f;

        public enum MovementState { walking, sprinting, air }
        [SerializeField] private MovementState state;

        [Header("Drag Settings")]
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 2f;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;

        public bool isGrounded;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("DebugDraw Settings")]
        public bool debug = false;

        private void Update()
        {
            InputManagement();
            SpeedControl();
            StateHandler();
            CameraMovement();
        }

        private void FixedUpdate()
        {
            isGrounded = IsGrounded();

            GroundMovement();
            ControlDrag();
        }

        private void StateHandler()
        {
            // Mode - Sprinting
            if (isGrounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            // Mode - Walking
            else if (isGrounded)
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
            }
            // Mode - Air
            else
            {
                state = MovementState.air;
            }
        }

        private void GroundMovement()
        {
            Vector3 dir = Vector3.zero;
            dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
            finalDir = dir.normalized;

            if (debug)
            {
                Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
            }

            if (isGrounded)
            {
                // Normal flat movement
                playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                // Airborne movement
                playerRigidbody.AddForce(finalDir * moveSpeed * 10f * airControlMultiplier, ForceMode.Force);
            }
        }

        private void ControlDrag()
        {
            if (isGrounded)
            {
                // Apply higher drag to help stop quickly on the ground
                playerRigidbody.drag = groundDrag;
            }
            else
            {
                // Apply lower drag in the air to allow smoother falling and air movement
                playerRigidbody.drag = airDrag;
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            // Limit velocity to max moveSpeed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
            }

            velocityMagnitude = flatVel.magnitude; // optional: for debugging or UI

            // Apply friction when idle on ground
            if (finalDir == Vector3.zero && isGrounded)
            {
                if (playerRigidbody.velocity.magnitude < minVelocity)
                {
                    playerRigidbody.velocity = Vector3.zero;
                }
                else
                {
                    // Apply friction opposite to velocity
                    Vector3 frictionForce = -playerRigidbody.velocity.normalized * groundDrag;
                    playerRigidbody.AddForce(frictionForce);
                }
            }
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
            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");

            //Gather input from the mouse for camera movement.
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");
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
