using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody playerRigidbody;
        public CapsuleCollider playerCollider;
        public Transform orientation;
        public PlayerInputHandler playerInputHandler;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float airControlMultiplier = 0.4f;
        [SerializeField] private float velocityMagnitude;
        private Vector3 finalDir;
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
        [SerializeField] private float extraGravityTimeAfterSlope = 0.3f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;     // Force applied on jump
        [SerializeField] private int maxJumps = 1;          // Max jump count (1 = single, 2 = double, etc.)

        public int jumpsLeft;                              // Number of remaining jumps.
        public bool pressedJump;

        [Header("Spawn Settings")]
        public Vector3 spawnPosition = Vector3.zero; // Custom spawn position
        public float spawnHeightOffset = 1.5f;       // Lift player slightly above ground
        public bool freezeMovementOnSpawn = true;    // Prevent control until grounded
        public bool canMove = false;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;
        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;
        public bool isGrounded;

        [Header("Slope Handling Settings")]
        [SerializeField] private float maxSlopeAngle = 45f;     // Max slope angle considered walkable.
        [SerializeField] private float maxSlideForce = 20f;     // Force applied when sliding down a steep slope.

        public bool isOnSlope;                                  // True, if on walkable slope.
        public bool isSlopeSteep;                               // True, if slope is too steep.
        public float slopeAngle;                                // Measured angle of current slope.
        private RaycastHit slopeHit;                            // Stores slope raycast hit info.
        private float lastTimeOnSlope;                          // Used for slope exit gravity handling.
        public float slideForce;

        [Header("Keybinds")]
        public KeyCode spawnKey = KeyCode.F;
        public KeyCode resetKey = KeyCode.R;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("DebugDraw Settings")]
        public bool debug = false;
        [System.Flags]
        public enum DebugTesting
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

        void Start()
        {
            jumpsLeft = maxJumps;   // Initialize jump count.

            // Initialize Rigidbody if not assigned
            if (!playerRigidbody)
            {
                playerRigidbody = GetComponent<Rigidbody>();
            }

            // Set initial position at spawn + height offset
            Vector3 startPos = spawnPosition + Vector3.up * spawnHeightOffset;
            ResetPlayerState(startPos, freezeMovementOnSpawn);
        }

        void Update()
        {
            if (!canMove)
            {
                return; // Wait until spawn animation or ground contact
            }

            // Debug hotkey to reset player
            if (Input.GetKey(spawnKey))
            {
                ResetPlayerState(spawnPosition + Vector3.up * spawnHeightOffset, false);
            }

            // Reset Player's Transform.Position (0, 0, 0)
            if (Input.GetKey(resetKey))
            {
                ResetPlayerState(Vector3.zero, false);
            }

            SpeedControl();
            StateHandler();


            // Jump key is pressed,
            if (playerInputHandler.jumpPressed)
            {
                pressedJump = true;     // Buffer jump input.
            }
        }

        private void FixedUpdate()
        {
            isGrounded = CheckGrounded();
            isOnSlope = OnSlope();

            // If grounded on slope, record timestamp for slope-exit gravity.
            if (isGrounded && slopeAngle >= 0 && isOnSlope)
            {
                lastTimeOnSlope = Time.time; // Record slope timestamp.
            }

            // Reset jumps when grounded.
            if (isGrounded)
            {
                jumpsLeft = maxJumps; // Reset jump count when grounded.
            }

            ExtraGravity();

            // Handle jump request.
            if (pressedJump)
            {
                pressedJump = false;
                if (isGrounded || jumpsLeft > 0)
                {
                    Jump(); // Perform jump if allowed
                }
            }

            GroundMovement();
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

        private void GroundMovement()
        {
            if (state == MovementState.dashing)
            {
                // Makeshift set of code used for [Dashing].
                //With it, the PlayerObj dashed a further distance.
                //Without it, the PlayerObj dashed a set distance.

                //Vector3 dashDir = orientation.forward;
                //rb.velocity = dashDir.normalized * dashSpeed;
                return;
            }

            // --- Calculate desired direction ---
            Vector3 dir = Vector3.zero;
            dir = orientation.forward * playerInputHandler.verticalInput + orientation.right * playerInputHandler.horizontalInput;
            finalDir = dir.normalized;  // Normalize to avoid diagonal speed boost.

            // Debug draw movement direction.
            if (debug)
            {
                Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
            }

            if (isGrounded && isOnSlope)
            {
                // Move along slope.
                playerRigidbody.AddForce(GetSlopeMoveDirection(dir) * moveSpeed * 10f, ForceMode.Force);
            }
            // --- Apply movement forces based on state ---
            else if (isGrounded)
            {
                // Normal flat ground movement.
                playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                // Airborne movement.
                playerRigidbody.AddForce(finalDir * moveSpeed * 10f * airControlMultiplier, ForceMode.Force);
            }
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

            if (isDashing && finalDir == Vector3.zero && CheckGrounded())
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

        private void ExtraGravity()
        {
            float extraGravityToApply = targetGravity - Physics.gravity.y;

            playerRigidbody.AddForce(Vector3.up * extraGravityToApply, ForceMode.Acceleration);

            // --- Apply extra gravity after leaving a slope ---
            if (!pressedJump && !(isGrounded && slopeAngle <= maxSlopeAngle) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
            {
                // Stack additional gravity force.
                playerRigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
            }
        }

        private void Jump()
        {
            // Reset vertical velocity.
            Vector3 velocity = playerRigidbody.velocity;
            velocity.y = 0;
            playerRigidbody.velocity = velocity;

            // Add upward jump force.
            playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);

            // If not grounded, consume a jump (for multi-jumps).
            if (!isGrounded)
            {
                //jumpsLeft--; // Reduce available jumps if mid-air.
            }

            jumpsLeft--; // Reduce available jumps if mid-air.
        }

        public void ResetPlayerState(Vector3 newPosition, bool freeze = false)
        {
            // --- RESET PHYSICS ---
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.position = newPosition;
            transform.position = newPosition; // Keep Transform in sync

            // Optional freeze
            if (freeze)
            {
                playerRigidbody.isKinematic = true;
                canMove = false;
                // Wait a short delay, then allow movement
                StartCoroutine(EnableMovementAfterDelay(0.5f));
            }
        }

        IEnumerator EnableMovementAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            playerRigidbody.isKinematic = false;
            canMove = true;
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

        private bool OnSlope()
        {
            // --- Get the origin point for slope check (bottom of the capsule) ---
            Vector3 origin = FeetPosition();

            // --- Calculate dynamic raycast distance ---
            float distance = playerCollider.height * 0.5f * transform.localScale.y + 0.3f;

            // --- Perform a downward raycast from feet position ---
            if (Physics.Raycast(origin, Vector3.down, out slopeHit, distance))
            {
                // --- Calculate the angle between the surface normal and world up ---
                slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

                // Return true only if within valid slope range
                return slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
            }

            // --- Reset slope angle if no valid hit detected ---
            slopeAngle = 0f;

            // True if on a slope and within acceptable angle; otherwise, false
            return false;
        }

        private Vector3 GetSlopeMoveDirection(Vector3 moveDir)
        {
            // Projects movement direction onto the slope plane using the surface normal
            // This ensures the player moves along the surface and not into it
            return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
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
