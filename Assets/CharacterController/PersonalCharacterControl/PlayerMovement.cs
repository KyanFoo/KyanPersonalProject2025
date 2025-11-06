using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody playerRigidbody;
        public CapsuleCollider playerCollider;

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

        [Header("Keybinds")]
        public KeyCode spawnKey = KeyCode.F;
        public KeyCode resetKey = KeyCode.R;

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
        }

        private void FixedUpdate()
        {
            isGrounded = CheckGrounded();
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
