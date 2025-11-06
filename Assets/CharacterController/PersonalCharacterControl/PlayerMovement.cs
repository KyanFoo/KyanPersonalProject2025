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
    }
}
