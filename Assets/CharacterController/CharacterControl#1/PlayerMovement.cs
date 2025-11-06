using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl1
{
    public class PlayerMovement : MonoBehaviour
    {
        /// <summary>
        /// Reference from Youtube Videos: [(Dave / GameDevelopment)]
        /// FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial
        /// SLOPE MOVEMENT, SPRINTING & CROUCHING - Unity Tutorial
        /// ADVANCED 3D DASH ABILITY in 11 MINUTES - Unity Tutorial
        /// </summary>

        [Header("Movement")]
        public float velocityMagnitude;

        private float moveSpeed;
        public float walkSpeed;
        public float sprintSpeed;
        public float dashSpeed;

        [Header("Dash Control")]
        public bool dashing;
        public float dashSpeedChangeFactor;
        public float maxYSpeed;

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

        public float targetGravity = -30f;

        public Rigidbody rb;

        public MovementState state;
        public enum MovementState
        {
            walking,
            sprinting,
            dashing,
            air
        }

        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;
        private MovementState lastState;
        private bool keepMomentum;

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
            if (state == MovementState.walking || state == MovementState.sprinting)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0f;
            }

            //TextStuff();
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
            if (dashing)
            {
                state = MovementState.dashing;
                desiredMoveSpeed = dashSpeed;
                speedChangeFactor = dashSpeedChangeFactor;
            }

            // Mode - Sprinting
            else if (grounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }

            // Mode - Walking
            else if (grounded)
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

        private void MovePlayer()
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

            // limit y vel
            if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
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

        public TextMeshProUGUI text_speed;
        public TextMeshProUGUI text_mode;
        private void TextStuff()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (OnSlope())
            {
                text_speed.SetText("Speed: " + Mathf.Round(rb.velocity.magnitude) + " / " + Mathf.Round(moveSpeed));
            }
            else
            {
                text_speed.SetText("Speed: " + Mathf.Round(flatVel.magnitude) + " / " + Mathf.Round(moveSpeed));
            }

            text_mode.SetText(state.ToString());
        }
    }
}