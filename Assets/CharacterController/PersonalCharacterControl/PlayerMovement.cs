using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody rb;
        public FirstPlayerCam firstPlayerCam;

        [Header("Spawn Settings")]
        public Vector3 spawnPosition = Vector3.zero; // Custom spawn position
        public float spawnHeightOffset = 1.5f;       // Lift player slightly above ground
        public bool freezeMovementOnSpawn = true;    // Prevent control until grounded
        public bool canMove = false;

        [Header("Keybinds")]
        public KeyCode spawnKey = KeyCode.Space;
        public KeyCode resetKey = KeyCode.R;

        void Start()
        {
            // Initialize Rigidbody if not assigned
            if (!rb)
            {
                rb = GetComponent<Rigidbody>();
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

        public void ResetPlayerState(Vector3 newPosition, bool freeze = false)
        {
            // --- RESET PHYSICS ---
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = newPosition;
            transform.position = newPosition; // Keep Transform in sync

            // Optional freeze
            if (freeze)
            {
                rb.isKinematic = true;
                canMove = false;
                // Wait a short delay, then allow movement
                StartCoroutine(EnableMovementAfterDelay(0.5f));
            }
        }

        IEnumerator EnableMovementAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            rb.isKinematic = false;
            canMove = true;
        }
    }
}
