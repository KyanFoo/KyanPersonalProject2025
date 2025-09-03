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
        [SerializeField] private Transform playerObj;
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private CapsuleCollider playerCollider;
        [SerializeField] Transform orientation;

        [Header("Input")]
        [SerializeField] private float sensX; 
        [SerializeField] private float sensY;
        private float mouseX;
        private float mouseY;
        private float xRotation;
        private float yRotation;
        float horizontalInput;
        float verticalInput;
        private Vector3 finalDir;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;     
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float airControlMultiplier = 0.4f;
        [SerializeField] public float velocityMagnitude;
        private float moveSpeed;
        private float minVelocity = 0.1f;

        public enum MovementState { walking, sprinting, air }
        [SerializeField] private MovementState state;

        [Header("Drag Settings")]
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 2f;

       [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private int maxJumps = 1;
        public int jumpsLeft;
        public bool pressedJump;

        [Header("GroundCheck Settings")]
        public LayerMask groundMask = 1;
        public float groundCheckDistance = 0.05f;

        public bool isGrounded;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Debug Settings")]
        public bool debug;

        private void Start()
        {
            jumpsLeft = maxJumps;
        }

        private void Update()
        {
            InputManagement();
            SpeedControl();
            Statehandler();
            CameraMovement();

            if (Input.GetKeyDown(jumpKey))
            {
                pressedJump = true; // Buffer jump input
            }
            //-----Explaination-----//
            // [Input.GetKey(jumpKey)] returns true every frame you hold down the key
            // Causing Jump() Function gets called multiple times, and jumpsLeft-- runs each time

            // [Input.GetKeyDown(jumpKey)] only return true on the frame the key is first pressed, preventing multi-jump spam.
        }
        private void FixedUpdate()
        {
            isGrounded = IsGrounded();

            if (isGrounded)
            {
                jumpsLeft = maxJumps;
            }

            if (pressedJump)
            {
                pressedJump = false;
                if (isGrounded || jumpsLeft > 0)
                {
                    Jump();
                }
            }

            GroundMovement();
            ControlDrag();
        }

        private void Statehandler()
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

        private void Jump()
        {
            // Cancel current vertical velocity
            Vector3 velocity = playerRigidbody.velocity;
            velocity.y = 0;
            playerRigidbody.velocity = velocity;

            // Add upward jump force
            playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
            jumpsLeft--;
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

            velocityMagnitude = flatVel.magnitude;

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
            //Adjust mouse Sensitivity.
            mouseX *= sensX * Time.deltaTime;
            mouseY *= sensY * Time.deltaTime;

            //Control the y axis cause player cam wants to look horizontally (Left & Right).
            yRotation += mouseX;

            //Control the x axis cause player cam wants to look vertically (Up & Down).
            xRotation -= mouseY;

            //Clamp to prevent overrotation.
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            // Apply rotation on the camera. Allowing it to move Left & Right [yRotation] and Up & Down [xRotation].
            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

            // Apply rotation on the PlayerObj. Allowing it to move Left & Right [yRotation].
            playerObj.rotation = Quaternion.Euler(0f, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        private Vector3 FeetPosition() // --- Calculate feet position for raycasts ---
        {
            Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            Vector3 feetPosition = playerRigidbody.position + sphereOffset;
            return feetPosition;
        }

        private bool IsGrounded() // --- Grounded check using SphereCast ---
        {
            Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
            bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);

            return isGrounded;
        }

        private void InputManagement()
        {
            // Gather inputs from the WASD keys for movement.
            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");

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
