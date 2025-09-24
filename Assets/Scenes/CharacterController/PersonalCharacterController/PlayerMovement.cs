using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using KyanPersonalProject2025.CharacterController1;
using TMPro;
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
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float airControlMultiplier = 0.4f;
        [SerializeField] private float velocityMagnitude;

        private float moveSpeed;
        private float minVelocity = 0.1f;

        public enum MovementState {walking, sprinting, dashing, air}
        [SerializeField] private MovementState state;

        [Header("Dash Control")]
        public bool isDashing;
        [SerializeField] private float dashSpeedChangeFactor;
        public float maxYSpeed;

        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;
        private MovementState lastState;
        private bool keepMomentum;

        [Header("Drag Settings")]
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 2f;
        [SerializeField] private float slidingDrag = 1f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private int maxJumps = 1;

        private int jumpsLeft;
        public bool pressedJump;

        [Header("GroundCheck Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.05f;

        public bool isGrounded;

        [Header("Slope Handling Settings")]
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private float maxSlideForce = 20f;

        public bool isOnSlope;
        public bool isSlopeSteep;
        public float slopeAngle;
        private RaycastHit slopeHit;
        private float lastTimeOnSlope;
        public float slideForce;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("DebugDraw Settings")]
        public bool debug = false;

        [System.Flags] public enum DebugTesting
        {
            None = 0,        // Always have a "None" option
            Debug1_FeetPosition = 1 << 0,   // 1
            Debug2_CastIsGrounded = 1 << 1,   // 2
            Debug3_IsGrounded = 1 << 2,   // 4
            Debug4_LerpMotion = 1 << 3,   // 8
            Debug5 = 1 << 4,   // 16
            Debug6 = 1 << 5,   // 32
            Debug7 = 1 << 6    // 64
        }
        public DebugTesting debugTypes;
        [Range(0f, 1f)][SerializeField] float debugCastProgress = 0f; // 0 = start, 1 = end

        private void Start()
        {
            jumpsLeft = maxJumps;
        }

        private void Update()
        {
            InputManagement();
            SpeedControl();
            StateHandler();
            CameraMovement();

            if (Input.GetKeyDown(jumpKey))
            {
                pressedJump = true;
            }
            TextStuff();
        }

        private void FixedUpdate()
        {
            isGrounded = IsGrounded();
            isOnSlope = OnSlope();
            isSlopeSteep = IsTooSteep();

            if (isGrounded && slopeAngle >= 0 && isOnSlope)
            {
                lastTimeOnSlope = Time.time; // Record slope timestamp
            }

            if (isGrounded)
            {
                jumpsLeft = maxJumps; // Reset jump count when grounded
            }

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
            // Mode - Sprinting
            if (isDashing)
            {
                state = MovementState.dashing;
                desiredMoveSpeed = dashSpeed;
                speedChangeFactor = dashSpeedChangeFactor;
            }
            // Mode - Sprinting
            else if (isGrounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }
            // Mode - Walking
            else if (isGrounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }
            // Mode - Air
            else
            {
                state = MovementState.air;

                if (desiredMoveSpeed < sprintSpeed)
                {
                    desiredMoveSpeed = walkSpeed;
                }
                else
                {
                    desiredMoveSpeed = sprintSpeed;
                }
            }
            bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

            if (lastState == MovementState.dashing)
            {
                keepMomentum = true;
            }

            if (desiredMoveSpeedHasChanged)
            {
                if (keepMomentum)
                {
                    StopAllCoroutines();
                    StartCoroutine(SmoothlyLerpMoveSpeed());
                }
                else
                {
                    StopAllCoroutines();
                    moveSpeed = desiredMoveSpeed;
                }
            }

            lastDesiredMoveSpeed = desiredMoveSpeed;
            lastState = state;
        }

        private float speedChangeFactor;
        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            // smoothly lerp movementSpeed to desired value
            float time = 0;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            float boostFactor = speedChangeFactor;

            while (time < difference)
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

                time += Time.deltaTime * boostFactor;

                yield return null;
            }

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

            Vector3 dir = Vector3.zero;
            dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
            finalDir = dir.normalized;

            if (debug)
            {
                Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
            }

            if (isGrounded && isSlopeSteep)
            {
                // Slide when on too steep a slope
                playerRigidbody.AddForce(GetSlopeSlideDirection(), ForceMode.Force);

                playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
            }
            else if (isGrounded && isOnSlope)
            {
                // Move along slope
                playerRigidbody.AddForce(GetSlopeMoveDirection(dir) * moveSpeed * 10f, ForceMode.Force);
            }
            else if (isGrounded)
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

            if (!isGrounded)
            {
                jumpsLeft--; // Reduce available jumps if mid-air
            }
        }

        private void ControlDrag()
        {

            if (isDashing)
            {
                // Skip friction when dashing.
                playerRigidbody.drag = 0f;
            }
            else if (isGrounded && isSlopeSteep)
            {
                // Apply lower drag when sliding
                playerRigidbody.drag = slidingDrag;
            }
            else if (isGrounded)
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
            // Ignore ALL speed control when dashing
            if (isDashing) return;

            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            // Limit velocity to max moveSpeed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
            }

            velocityMagnitude = flatVel.magnitude; // optional: for debugging or UI

            // limit y vel
            if (maxYSpeed != 0 && playerRigidbody.velocity.y > maxYSpeed)
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, maxYSpeed, playerRigidbody.velocity.z);
            }

            // Apply friction when idle on ground
            if (finalDir == Vector3.zero && isGrounded)
            {
                Debug.Log("Control2");
                if (playerRigidbody.velocity.magnitude < minVelocity)
                {
                    Debug.Log("Control3");
                    playerRigidbody.velocity = Vector3.zero;
                }
                else
                {
                    Debug.Log("Control4");
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
