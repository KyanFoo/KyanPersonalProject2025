using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController1
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float velocityMagnitude;

        private float moveSpeed;
        public float walkSpeed;
        public float sprintSpeed;

        [Header("Drag")]
        public float groundDrag;

        [Header("Jump Control")]
        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        public bool readyToJump;

        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        public bool grounded;

        [Header("Slope Handling")]
        public float maxSlopeAngle;
        private RaycastHit slopeHit;
        private bool exitingSlope;
        public float slopeAngle;

        public Transform orientation;

        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        float targetGravity = -30f;

        public Rigidbody rb;

        public MovementState state;
        public enum MovementState
        {
            walking,
            sprinting,
            air
        }

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
            StateHandler();

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
            // Calculate extra gravity needed
            float extraGravity = targetGravity - Physics.gravity.y;
            rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);

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

        private void StateHandler()
        {
            // Mode - Sprinting
            if (grounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }

            // Mode - Walking
            else if (grounded)
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

        private void MovePlayer()
        {
            // Calculate movement direction.
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            // on slope.
            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

                if (rb.velocity.y > 0)
                {
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }

            // on ground.
            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            }

            // in air.
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }

            // turn gravity off while on slope.
            rb.useGravity = !OnSlope();
        }

        private void SpeedControl()
        {
            // limit speed on slope.
            if (OnSlope() && !exitingSlope)
            {
                if (rb.velocity.magnitude > moveSpeed)
                {
                    rb.velocity = rb.velocity.normalized * moveSpeed;
                }
            }

            // limiting speed on ground or in air.
            else
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
        }

        private void Jump()
        {
            exitingSlope = true;

            // Reset y velocity.
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void ResetJump()
        {
            readyToJump = true;

            exitingSlope = false;
        }

        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                slopeAngle = angle;
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        }
    }
}