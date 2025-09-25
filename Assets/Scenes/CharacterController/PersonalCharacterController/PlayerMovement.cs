using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

        [Header("Reference")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;    // Reference to Cinemachine camera for player view.
        [SerializeField] private Transform playerBody;                      // The visible body of the player (rotates with camera Y-axis).
        [SerializeField] private Rigidbody playerRigidbody;                 // Physics Rigidbody attached to the player for physics-based movement.
        [SerializeField] private CapsuleCollider playerCollider;            // Capsule collider used for collision and ground checks.
        [SerializeField] private Transform orientation;                     // Used to align player the direction its facing, usually aligned with camera.

        [Header("Input Settings")]
        [SerializeField] private float sensX = 150f;   // Mouse sensitivity for X-axis (horizontal rotation).
        [SerializeField] private float sensY = 150f;   // Mouse sensitivity for Y-axis (vertical rotation).

        private float mouseX;                   // Stores raw mouse X input for movement values.
        private float mouseY;                   // Stores raw mouse Y input for movement values.
        private float verticalInput;            // Input from W/S or Up/Down.
        private float horizontalInput;          // Input from A/D or Left/Right.

        private float xRotation;                // Camera rotation up/down (pitch).
        private float yRotation;                // Camera rotation left/right (yaw).

        private Vector3 finalDir;               // Final calculated move direction vector.

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;                  // Speed while walking.
        [SerializeField] private float sprintSpeed = 10f;               // Speed while sprinting.
        [SerializeField] private float dashSpeed = 20f;                 // Speed boost when dashing.
        [SerializeField] private float airControlMultiplier = 0.4f;     // Movement control when airborne.
        [SerializeField] private float velocityMagnitude;               /// Debug / UI tracker for speed.

        private float moveSpeed;                                        // Current active movement speed.
        private float minVelocity = 0.1f;                               // Used for friction stop when velocity is tiny.

        public enum MovementState {walking, sprinting, dashing, air }   // Movement state enum.
        [SerializeField] private MovementState state;                   // Current movement state.

        [Header("Dash Control")]
        public bool isDashing;                                  // True, if dash is active.
        [SerializeField] private float dashSpeedChangeFactor;   // Controls acceleration into dash speed.
        public float maxYSpeed;                                 // Clamp for upward velocity.

        private float desiredMoveSpeed;                         // Target speed based on state.
        private float lastDesiredMoveSpeed;                     // Last frame's Speed value.
        private MovementState lastState;                        // Last frame's movement state.
        private bool keepMomentum;                              // Used to preserve momentum after dashing.
        private float speedChangeFactor;                        // Controls transition speed between states

        [Header("Drag Settings")]
        [SerializeField] private float groundDrag = 6f;     // Drag when grounded.
        [SerializeField] private float airDrag = 2f;        // Drag while airborne.
        [SerializeField] private float slidingDrag = 1f;    // Drag while sliding on steep slope.

        [Header("Gravity Control Settings")]
        [SerializeField] private float targetGravity = -24f;                // Custom gravity strength (replaces Unity’s -9.81).
        [SerializeField] private float extraGravity = 2f;                   // Extra downward force applied after leaving a slope.
        [SerializeField] private float extraGravityTimeAfterSlope = 0.3f;   // Grace period after leaving a slope.

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;     // Force applied on jump
        [SerializeField] private int maxJumps = 1;          // Max jump count (1 = single, 2 = double, etc.)

        public int jumpsLeft;                              // Number of remaining jumps.
        public bool pressedJump;                            // Input buffer for jump key press.

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;                  // Layer mask used to define ground.
        [SerializeField] private float groundCheckDistance = 0.05f;     // Ground detection distance.

        public bool isGrounded;                                         // True, if touching the ground.

        [Header("Slope Handling Settings")]
        [SerializeField] private float maxSlopeAngle = 45f;     // Max slope angle considered walkable.
        [SerializeField] private float maxSlideForce = 20f;     // Force applied when sliding down a steep slope.

        public bool isOnSlope;                                  // True, if on walkable slope.
        public bool isSlopeSteep;                               // True, if slope is too steep.
        public float slopeAngle;                                // Measured angle of current slope.
        private RaycastHit slopeHit;                            // Stores slope raycast hit info.
        private float lastTimeOnSlope;                          // Used for slope exit gravity handling.
        public float slideForce;                                // Used for force applied when sliding down a steep slope.

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;         // Jump key.
        public KeyCode sprintKey = KeyCode.LeftShift;   // Sprint key.

        [Header("DebugDraw Settings")]
        public bool debug = false;              // Master toggle for debug visuals.

        [System.Flags] public enum DebugTesting // Flags enum for selective debug toggling.
        {
            None = 0,                                                   // 0, Default, Always have a "None" option.
            Debug1_FeetPosition = 1 << 0,                               // 1. Show [FeetPosition] sphere.
            Debug2_CastIsGrounded = 1 << 1,                             // 2. Show [IsGrounded] origin sphere.
            Debug3_IsGrounded = 1 << 2,                                 // 4, Show [IsGrounded] Grounded check sphere.
            Debug4_LerpMotion = 1 << 3,                                 // 8, Show [IsGrounded] Grounded check sphere moving from cast start > end.
            Debug5 = 1 << 4,                                            // 16, Reserved.
            Debug6 = 1 << 5,                                            // 32, Reserved.
            Debug7 = 1 << 6                                             // 64, Reserved.
        }
        public DebugTesting debugTypes;                                 // Which debug modes are active
        [Range(0f, 1f)][SerializeField] float debugCastProgress = 0f;   // Used for animating debug sphere motion between cast start/end

        private void Start()
        {
            jumpsLeft = maxJumps;   // Initialize jump count.
        }

        private void Update()
        {
            InputManagement();      // Collect input each frame.
            SpeedControl();         // Clamp velocity & friction.
            StateHandler();         // Determine current movement state,
            CameraMovement();       // Apply camera rotation from mouse.

            // Jump key is pressed,
            if (Input.GetKeyDown(jumpKey))
            {
                pressedJump = true;     // Buffer jump input.
            }
            TextStuff();    // Update UI text (speed/mode).
        }

        private void FixedUpdate()
        {
            isGrounded = IsGrounded();      // Check ground.
            isOnSlope = OnSlope();          // Check if on slope.
            isSlopeSteep = IsTooSteep();    // Check if slope is too steep.

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

            ExtraGravity(); // Apply gravity modifications.

            // Handle jump request.
            if (pressedJump)
            {
                pressedJump = false;
                if (isGrounded || jumpsLeft > 0)
                {
                    Jump(); // Perform jump if allowed
                }
            }

            GroundMovement();   // Apply movement force.
            ControlDrag();      // Apply proper drag.
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
            dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
            finalDir = dir.normalized;  // Normalize to avoid diagonal speed boost.

            // Debug draw movement direction.
            if (debug)
            {
                Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
            }

            // --- Apply movement forces based on state ---
            if (isGrounded && isSlopeSteep)
            {
                // Sliding on steep slope.
                playerRigidbody.AddForce(GetSlopeSlideDirection(), ForceMode.Force);

                playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
            }
            else if (isGrounded && isOnSlope)
            {
                // Move along slope.
                playerRigidbody.AddForce(GetSlopeMoveDirection(dir) * moveSpeed * 10f, ForceMode.Force);
            }
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

        private void ExtraGravity()
        {
            // --- Calculate gravity difference based on targetGravity ---
            // Example: if Physics.gravity.y = -9.81 and targetGravity = -20,
            // extraGravityToApply = -20 - (-9.81) = -10.19
            float extraGravityToApply = targetGravity - Physics.gravity.y;

            // Apply base gravity adjustment so the player always experiences targetGravity
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

        private void ControlDrag()
        {

            if (isDashing)
            {
                // Disable drag during dashing for max distance.
                playerRigidbody.drag = 0f;
            }
            else if (isGrounded && isSlopeSteep)
            {
                // Sliding drag (less friction)
                playerRigidbody.drag = slidingDrag;
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
            // Ignore ALL speed control when dashing
            if (isDashing) return;

            // Flattened velocity (ignoring Y).
            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            // Clamp horizontal velocity to current moveSpeed.
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
            }

            // Store velocity magnitude (for debug/UI).
            velocityMagnitude = flatVel.magnitude;

            // Clamp upward velocity if maxYSpeed is set.
            if (maxYSpeed != 0 && playerRigidbody.velocity.y > maxYSpeed)
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, maxYSpeed, playerRigidbody.velocity.z);
            }

            // Apply friction when idle on ground
            if (finalDir == Vector3.zero && isGrounded)
            {
                if (playerRigidbody.velocity.magnitude < minVelocity)
                {
                    // Stop completely when very slow.
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

            yRotation += mouseX;
            xRotation -= mouseY;

            //Prevent overrotation.
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            // Apply rotation to camera
            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            //Rotate the Player.
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
            //-----Explaination-----//
            //Set the player's rotation directly
            //Here’s how you can modify your script to unify both camera and player rotations using the same variables (xRotation and yRotation)
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

        private bool OnSlope()
        {

            // Get the origin point for the slope check (bottom of capsule)
            Vector3 origin = FeetPosition();

            float distance = playerCollider.height * 0.5f * transform.localScale.y + 0.3f;

            // Perform a raycast straight downward to detect the surface below the player
            if (Physics.Raycast(origin, Vector3.down, out slopeHit, distance))
            {
                // Calculate the angle between the hit normal and world up (i.e., how steep the surface is)
                slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

                return slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
            }

            // If nothing was hit, reset slope angle and return false
            slopeAngle = 0f;

            // True if on a slope and within acceptable angle; otherwise, false
            return false;
        }

        private bool IsTooSteep()
        {
            // True if too steep to walk on; otherwise, false
            return slopeAngle > maxSlopeAngle;
        }

        private Vector3 GetSlopeSlideDirection()
        {
            // Get the origin point for the slope check (bottom of capsule)
            Vector3 origin = FeetPosition();

            float distance = playerCollider.height * 0.5f * transform.localScale.y + 0.3f;

            // Perform a raycast straight downward to detect the surface below the player
            if (Physics.Raycast(origin, Vector3.down, out slopeHit, distance))
            {
                // Calculate the angle between the hit normal and world up (i.e., how steep the surface is)
                slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

                if (slopeAngle > maxSlopeAngle)
                {
                    float t = Mathf.InverseLerp(maxSlopeAngle, 70f, slopeAngle); // Tweak 70f if needed
                    slideForce = Mathf.Lerp(30f, maxSlideForce, t); // Adds a minimum slide nudge

                    //slideForce = Mathf.Lerp(0f, maxSlideForce, (slopeAngle - maxSlopeAngle) / (70f - maxSlopeAngle));
                    Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                    return slideDir * slideForce;
                }
            }

            return Vector3.zero; // No sliding needed
        }

        private Vector3 GetSlopeMoveDirection(Vector3 moveDir)
        {
            // Projects movement direction onto the slope plane using the surface normal
            // This ensures the player moves along the surface and not into it
            return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
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
                if (debugTypes.HasFlag(DebugTesting.Debug1_FeetPosition))
                {
                    //Draw a WireSphere to show the FeetPosition point. ###
                    Gizmos.DrawWireSphere(FeetPosition(), playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug2_CastIsGrounded))
                {
                    //Draw a WireSphere to show the IsGrounded point slightly above the FeetPositon to avoid clipping. ###
                    Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(checkOrigin, playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug3_IsGrounded))
                {
                    // Draw a WireSphere to visualize a FeetPosition Collidier for Player Prefab for Ground & Slope Check.
                    Vector3 feetOffset = -1 * groundCheckDistance * transform.up;

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
                }

                if (debugTypes.HasFlag(DebugTesting.Debug4_LerpMotion))
                {
                    // 1. SphereCast start point (above feet to avoid clipping)
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
        public TextMeshProUGUI text_speed;
        public TextMeshProUGUI text_mode;
        private void TextStuff()
        {
            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            if (OnSlope())
            {
                text_speed.SetText("Speed: " + Mathf.Round(playerRigidbody.velocity.magnitude) + " / " + Mathf.Round(moveSpeed));
            }
            else
            {
                text_speed.SetText("Speed: " + Mathf.Round(flatVel.magnitude) + " / " + Mathf.Round(moveSpeed));
            }

            text_mode.SetText(state.ToString());
        }
    }
}
