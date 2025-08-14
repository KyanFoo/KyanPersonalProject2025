using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController1
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float velocityMagnitude;
        public float moveSpeed;

        [Header("Drag")]
        public float groundDrag;

        [Header("Jump Control")]
        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        public bool readyToJump;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;

        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        public bool grounded;

        public Transform orientation;

        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        public Rigidbody rb;

        // Start is called before the first frame update.
        private void Start()
        {
            // Assign [RigidBody] and freeze rotation.
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            // Set player to be able to jump.
            readyToJump = true;
        }

        // Update is called once per frame.
        private void Update()
        {
            // ground check.
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            MyInput();
            SpeedControl();

            // Handle drag.
            if (grounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0f;
            }
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MyInput()
        {
            // Get [keyboard input].
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            // Jump input.
            if (Input.GetKey(jumpKey) && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        private void MovePlayer()
        {
            // Calculate movement direction.
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            }
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);

                velocityMagnitude = flatVel.magnitude;
            }
        }

        private void Jump()
        {
            // Reset y velocity.
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void ResetJump()
        {
            readyToJump = true;
        }
    }
}