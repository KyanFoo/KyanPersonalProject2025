using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl4
{
    public class PlayerMovement : MonoBehaviour
    {
        /// <summary>
        /// Reference from Youtube Videos: [(Gigabyte)]
        /// Unity 2021 - Rigidbody Player Controller Full Explanation - Part 1-3
        /// </summary>

        private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

        [SerializeField] private Transform playerCamera;
        [SerializeField] private Transform orientation;

        [Space]
        public LayerMask groundMask = 1;
        public float groundCheckDistance = 0.05f;

        [Space]
        public float jumpForce = 5;
        public int maxJumps = 1;
        public float walkForce = 3;
        public float walkSpeed = 3;
        public float airMovementMultiplier = 0.15f;

        [Space]
        public float friction = 10f;
        public float minVelocity = 0.1f;
        public float maxSlopeAngle = 45;
        public float slopeRayLength = 0.5f;

        [Space]
        public float gravityMultiplier = 1f;
        public float extraGravity = 2f;
        public float extraGravityTimeAfterSlope = 0.3f;

        [Space]
        public bool debug;

        public Rigidbody playerRigidbody;
        public CapsuleCollider playerCollider;

        private int jumpsLeft;
        private bool isGrounded;

        private bool pressedJump;
        private Vector3 yaw;

        private float slopeAngle;
        private float lastTimeOnSlope;

        private Vector3 movementInput;
        private float forward;
        private float sideways;
        private float timeOfLastMove;

        public float sphereSize;

        void Start()
        {
            // Reset jumps
            jumpsLeft = maxJumps;

            // Assign Rigidbody and PlayerCollider.
            playerRigidbody = GetComponent<Rigidbody>();
            playerCollider = GetComponent<CapsuleCollider>();
        }

        void Update()
        {
            // WASD inputs
            sideways = Input.GetAxisRaw("Horizontal");
            forward = Input.GetAxisRaw("Vertical");
            movementInput.x = sideways;
            movementInput.z = forward;

            // Jump input
            if (Input.GetButtonDown("Jump"))
            {
                pressedJump = true;
            }
        }

        private void FixedUpdate()
        {
            // NOTE:
            // Applying [hysics to the rigidbody must be in FixedUpdate.
            // Otherwise it will cause (jittery) / (unpredictable) movement.
            RotateBodyToLookingDirection();

            // Ground & slope checks.
            isGrounded = IsGrounded();
            slopeAngle = SlopeAngle();

            // Track slope timer.
            if (isGrounded && slopeAngle >= 0 && OnFloor())
            {
                lastTimeOnSlope = Time.time;
            }

            // Reset jump count when grounded.
            if (isGrounded)
            {
                jumpsLeft = maxJumps;
            }

            // Apply extra gravity forces.
            ExtraGravity();

            // Handle jump.
            if (pressedJump)
            {
                pressedJump = false;

                if (isGrounded || jumpsLeft > 0)
                {
                    Jump();
                }
            }

            // Handle movement & friction.
            Movement();
            FrictionForces();
        }

        private void RotateBodyToLookingDirection() // --- Rotate body based on camera yaw only (ignore pitch/roll) ---
        {
            // Only use Y rotation from camera.
            yaw.y = playerCamera.eulerAngles.y;

            // Apply rotation to player body.
            transform.eulerAngles = (yaw);
        }

        private void Jump() // --- Jump logic ---
        {
            // Reset vertical velocity so jumps are consistent.
            Vector3 currentVelocity = playerRigidbody.velocity;
            currentVelocity.y = 0;
            playerRigidbody.velocity = currentVelocity;

            // Add vertical impulse (Jump).
            playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
            jumpsLeft--;
        }

        private void Movement() // --- Movement logic ---
        {
            Vector3 currentVelocity = playerRigidbody.velocity;

            // Base movement vector from input.
            Vector3 finalDir = (transform.forward * forward + transform.right * sideways).normalized;

            if (isGrounded && OnFloor())
            {
                Vector3 dir = Vector3.zero;
                dir += orientation.transform.forward * forward;
                dir += orientation.transform.right * sideways;
                finalDir = dir.normalized;

                if (debug)
                {
                    // Debug line showing intended movement direction.
                    Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
                }
            }

            if (!isGrounded)
            {
                finalDir *= airMovementMultiplier;
            }

            if (OnFloor())
            {
                playerRigidbody.AddForce(walkForce * finalDir);
            }

            // Clamp speed to walkSpeed.
            if (currentVelocity.magnitude > walkSpeed)
            {
                if (isGrounded && OnFloor())
                {
                    // Clamp full velocity when grounded.
                    Vector3 clamped = Vector3.ClampMagnitude(playerRigidbody.velocity, walkSpeed);
                    playerRigidbody.velocity = clamped;
                }

                else if (!isGrounded)
                {
                    // Clamp horizontal velocity only when in air.
                    Vector3 horizontalClamped = Vector3.ClampMagnitude(new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z), walkSpeed);
                    playerRigidbody.velocity = horizontalClamped + Vector3.up * playerRigidbody.velocity.y;
                }
            }
        }

        private void ExtraGravity() // --- Extra gravity logic ---
        {
            // Add extra gravity in general.
            playerRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
            
            // Apply extra gravity after leaving a slope for a short period.
            if (!pressedJump && !(isGrounded && OnFloor()) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
            {
                playerRigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
            }
        }

        private void FrictionForces() // --- Friction logic when not moving ---
        {
            // Only apply when standing still on ground.
            if (movementInput != Vector3.zero) return;
            if (!OnFloor() || !isGrounded) return;

            if (playerRigidbody.velocity.magnitude < minVelocity)
            {
                // Stop completely if velocity is tiny.
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                // Apply opposite force to slow down gradually.
                playerRigidbody.AddForce(-1 * friction * playerRigidbody.velocity.normalized);
            }
        }

        private Vector3 FeetPosition() // --- Calculate feet position for raycasts ---
        {
            Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            Vector3 feetPosition = playerRigidbody.position + sphereOffset;
            return feetPosition;
        }

        private float SlopeAngle() // --- Detect slope angle under player ---
        {
            float distance = slopeRayLength + playerCollider.height * transform.localScale.y * 0.5f;
            bool hit = Physics.Raycast(playerRigidbody.position, Vector3.down, out RaycastHit info, distance);

            if (hit)
            {
                return Vector3.Angle(Vector3.up, info.normal);
            }

            return -1;
        }

        private bool OnFloor() // --- Check if slope is walkable ---
        {
            return SlopeAngle() <= maxSlopeAngle;
        }

        private bool IsGrounded() // --- Grounded check using SphereCast ---
        {
            Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
            bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);

            return isGrounded;
        }

        private void OnDrawGizmos() // --- Debug gizmos for visualizing ground check ---
        {
            if (debug)
            {
                // Draw a WireSphere to visualize a FeetPosition Collidier for Player Prefab for Ground & Slope Check.
                //Vector3 feetOffset = -1 * groundCheckDistance * transform.up;
                //Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);

                //---Testing---//
                Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
                Vector3 feetPosition = playerRigidbody.position + sphereOffset;

                // Draw a colour sphere at the [Feet Position] of the Player Prefab.
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(feetPosition, sphereSize);

                // Draw a line between the Player Prefab centre position and [Feet Position].
                //Gizmos.color = Color.yellow;
                //Gizmos.DrawLine(playerRigidbody.position, feetPosition); // Line from center to feet
            }
        }
    }
}
