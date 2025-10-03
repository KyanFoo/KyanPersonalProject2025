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

        private Vector3 finalDir;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float airControlMultiplier = 0.4f;
        [SerializeField] private float velocityMagnitude;

        private float moveSpeed;
        private float minVelocity = 0.1f;

        [Header("MovementState Settings")]
        [SerializeField] private MovementState state;
        public enum MovementState { walking, sprinting, dashing, air }

        [Header("Dash Control")]
        public bool isDashing;
        [SerializeField] private float dashSpeedChangeFactor;
        public float maxYSpeed;

        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;
        private MovementState lastState;
        private bool keepMomentum;
        private float speedChangeFactor;

        [Header("Drag Settings")]
        [SerializeField] private float groundDrag = 5f;
        [SerializeField] private float airDrag = 2f;

        [Header("Gravity Control Settings")]
        [SerializeField] private float targetGravity = -24f;
        [SerializeField] private float extraGravity = 2f;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;
        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;
        public bool isGrounded;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("DebugDraw Settings")]
        public bool debug = false;
        [System.Flags] public enum DebugTesting
        {
            None = 0,
            Debug1_FeetPosition = 1 << 0,
            Debug2_CastIsGrounded = 1 << 1,
            Debug3_IsGrounded = 1 << 2,
            Debug4_LerpMotion = 1 << 3,
            Debug5 = 1 << 4,
            Debug6 = 1 << 5,
            Debug7 = 1 << 6,
        }
        public DebugTesting debugTypes;
        [Range(0f, 1f)][SerializeField] float debugCastProgress = 0f;

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        public void Update()
        {
            InputManagement();
            SpeedControl();
            StateHandler();
            CameraMovement();
        }

        private void FixedUpdate()
        {
            isGrounded = CheckGrounded();

            ExtraGravity();

            ControlDrag();
        }

        private void StateHandler()
        {
            // --- Dashing State ---
            if (isDashing)
            {
                state = MovementState.dashing;
                desiredMoveSpeed = dashSpeed;
                speedChangeFactor = dashSpeedChangeFactor;
            }

            // --- Sprinting State ---
            else if (isGrounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }

            // --- Walking State ---
            else if (isGrounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }

            // --- Air State (jumping/falling) ---
            else
            {
                state = MovementState.air;

                // If current desired speed is below sprint threshold, default to walk speed.
                if (desiredMoveSpeed < sprintSpeed)
                {
                    desiredMoveSpeed = walkSpeed;
                }
                else
                {
                    desiredMoveSpeed = sprintSpeed;
                }
            }

            // Check if desired speed has changed compared to last frame.
            bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

            // Preserve momentum when coming out of a dash.
            if (lastState == MovementState.dashing)
            {
                keepMomentum = true;
            }

            // Handle speed transitions.
            if (desiredMoveSpeedHasChanged)
            {
                if (keepMomentum)
                {
                    // Smoothly transition to new speed.
                    StopAllCoroutines();
                    StartCoroutine(SmoothlyLerpMoveSpeed());
                }
                else
                {
                    // Instantly snap to new speed.
                    StopAllCoroutines();
                    moveSpeed = desiredMoveSpeed;
                }
            }

            // Store state for next frame checks.
            lastDesiredMoveSpeed = desiredMoveSpeed;
            lastState = state;
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            // smoothly lerp movementSpeed to desired value
            float time = 0;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            float boostFactor = speedChangeFactor;

            while (time < difference)
            {
                // Lerp speed based on elapsed time.
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

                time += Time.deltaTime * boostFactor;

                yield return null;
            }

            // Final snap to desired value.
            moveSpeed = desiredMoveSpeed;
            speedChangeFactor = 1f;
            keepMomentum = false;
        }

        private void ExtraGravity()
        {
            float extraGravityToApply = targetGravity - Physics.gravity.y;

            playerRigidbody.AddForce(Vector3.up * extraGravityToApply, ForceMode.Acceleration);
        }


        private void ControlDrag()
        {
            if (isDashing)
            {
                // Disable drag during dashing for max distance.
                playerRigidbody.drag = 0f;
            }
            else if (isGrounded)
            {
                // Normal ground drag, help stop quickly on the ground.
                playerRigidbody.drag = groundDrag;
            }
            else
            {
                // Air drag, allowing smoother falling and air movement.
                playerRigidbody.drag = airDrag;
            }
        }

        private void SpeedControl()
        {
            if (isDashing)
            {
                return;
            }

            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);

                velocityMagnitude = flatVel.magnitude;
            }

            if (maxYSpeed != 0 && playerRigidbody.velocity.y > maxYSpeed)
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, maxYSpeed, playerRigidbody.velocity.z);
            }

            if (finalDir == Vector3.zero && CheckGrounded())
            {
                if (playerRigidbody.velocity.magnitude < minVelocity)
                {
                    playerRigidbody.velocity = Vector3.zero;
                }
                else
                {
                    Vector3 frictionForce = -1 * playerRigidbody.velocity.normalized * groundDrag;
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

        private void OnDrawGizmos() // --- Debug gizmos for visualizing ground check ---
        {
            if (debug)  // Only run if debug mode is enabled.
            {
                if (debugTypes.HasFlag(DebugTesting.Debug1_FeetPosition)) // Show the FeetPosition as a wire sphere.
                {
                    //Draw a WireSphere to show the FeetPosition point.
                    Gizmos.DrawWireSphere(FeetPosition(), playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug2_CastIsGrounded)) // Show the SphereCast origin (slightly above feet).
                {
                    //Draw a WireSphere to show the IsGrounded point slightly above the FeetPositon to avoid clipping. ###
                    Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(checkOrigin, playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug3_IsGrounded)) // Show the expected grounded check endpoint.
                {
                    // Draw a WireSphere to visualize a FeetPosition Collidier for Player Prefab for Ground & Slope Check.
                    Vector3 feetOffset = -1 * groundCheckDistance * transform.up;

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug4_LerpMotion)) // Visualize the entire SphereCast motion using interpolation.
                {
                    // SphereCast start point (above feet to avoid clipping)
                    Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;
                    Vector3 feetOffset = -1 * groundCheckDistance * transform.up;

                    // ----- Motion simulation -----
                    // debugCastProgress = 0 > start, 1 > end
                    Vector3 movingSpherePos = Vector3.Lerp(checkOrigin, FeetPosition() + feetOffset, debugCastProgress);
                    Gizmos.color = Color.cyan; // moving sphere color
                    Gizmos.DrawWireSphere(movingSpherePos, playerCollider.radius * transform.localScale.x);
                }

            }
        }
    }
}
