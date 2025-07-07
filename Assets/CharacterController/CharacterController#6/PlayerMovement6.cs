using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController6
{
    public class PlayerMovement6 : MonoBehaviour
    {
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

        void Start()
        {
            jumpsLeft = maxJumps;
            playerRigidbody = GetComponent<Rigidbody>();
            playerCollider = GetComponent<CapsuleCollider>();
        }

        void Update()
        {
            sideways = Input.GetAxisRaw("Horizontal");
            forward = Input.GetAxisRaw("Vertical");
            movementInput.x = sideways;
            movementInput.z = forward;

            if (Input.GetButtonDown("Jump"))
            {
                pressedJump = true;
            }
        }

        private void FixedUpdate()
        {
            RotateBodyToLookingDirection();

            isGrounded = IsGrounded();
            slopeAngle = SlopeAngle();

            if (isGrounded && slopeAngle >= 0 && OnFloor())
            {
                lastTimeOnSlope = Time.time;
            }

            if (isGrounded)
            {
                jumpsLeft = maxJumps;
            }

            ExtraGravity();

            if (pressedJump)
            {
                pressedJump = false;

                if (isGrounded || jumpsLeft > 0)
                {
                    Jump();
                }
            }

            Movement();
            FrictionForces();
        }

        private void RotateBodyToLookingDirection()
        {
            yaw.y = playerCamera.eulerAngles.y;
            transform.eulerAngles = (yaw);
        }

        private void Jump()
        {
            Vector3 currentVelocity = playerRigidbody.velocity;
            currentVelocity.y = 0;
            playerRigidbody.velocity = currentVelocity;

            playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
            jumpsLeft--;
        }

        private void Movement()
        {
            Vector3 currentVelocity = playerRigidbody.velocity;
            Vector3 finalDir = (transform.forward * forward + transform.right * sideways).normalized;

            if (isGrounded && OnFloor())
            {
                Vector3 dir = Vector3.zero;
                dir += orientation.transform.forward * forward;
                dir += orientation.transform.right * sideways;
                finalDir = dir.normalized;

                if (debug)
                {
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

            if (currentVelocity.magnitude > walkSpeed)
            {
                if (isGrounded && OnFloor())
                {
                    Vector3 clamped = Vector3.ClampMagnitude(playerRigidbody.velocity, walkSpeed);
                    playerRigidbody.velocity = clamped;
                }

                else if (!isGrounded)
                {
                    Vector3 horizontalClamped = Vector3.ClampMagnitude(new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z), walkSpeed);
                    playerRigidbody.velocity = horizontalClamped + Vector3.up * playerRigidbody.velocity.y;
                }
            }
        }

        private void ExtraGravity()
        {
            // Add extra gravity in general
            playerRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

            if (!pressedJump && !(isGrounded && OnFloor()) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
            {
                playerRigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
            }
        }

        private void FrictionForces()
        {
            if (movementInput != Vector3.zero) return;
            if (!OnFloor() || !isGrounded) return;

            if (playerRigidbody.velocity.magnitude < minVelocity)
            {
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                playerRigidbody.AddForce(-1 * friction * playerRigidbody.velocity.normalized);
            }
        }

        private Vector3 FeetPosition()
        {
            Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            Vector3 feetPosition = playerRigidbody.position + sphereOffset;
            return feetPosition;
        }

        private float SlopeAngle()
        {
            float distance = slopeRayLength + playerCollider.height * transform.localScale.y * 0.5f;
            bool hit = Physics.Raycast(playerRigidbody.position, Vector3.down, out RaycastHit info, distance);

            if (hit)
            {
                return Vector3.Angle(Vector3.up, info.normal);
            }

            return -1;
        }

        private bool OnFloor()
        {
            return SlopeAngle() <= maxSlopeAngle;
        }

        private bool IsGrounded()
        {
            Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
            bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);

            return isGrounded;
        }

        private void OnDrawGizmos()
        {
            if (debug)
            {
                Vector3 feetOffset = -1 * groundCheckDistance * transform.up;
                Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
            }
        }
    }
}
