using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl3
{
    public class PlayerMovement : MonoBehaviour
    {
        /// <summary>
        /// Reference from Youtube Videos: [(Plai)]
        /// Rigidbody FPS Controller Tutorial #1-4
        /// </summary>

        float playerHeight = 2f;
        [SerializeField] Transform orientation;

        [Header("Movement")]
        public float moveSpeed;
        public float movementMultiplier = 10f;
        [SerializeField] float airMultiplier = 0.4f;

        [Header("Sprinting")]
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float sprintSpeed = 6f;
        [SerializeField] float acceleration = 10f;

        [Header("Jump")]
        public float jumpForce = 5f;

        [Header("Keybind")]
        [SerializeField] KeyCode jumpKey = KeyCode.Space;
        [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Drag Control")]
        float groundDrag = 6f;
        float airDrag = 2f;

        float horizontalMovement;
        float verticalMovement;

        [Header("Ground Check")]
        [SerializeField] Transform groundCheck;
        [SerializeField] LayerMask groundMask;
        public bool isGrounded;
        float groundDistance = 0.4f;

        Vector3 moveDirection;
        Vector3 slopeMoveDirection;

        Rigidbody rb;

        [Header("Slope Check")]
        RaycastHit slopeHit;

        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
            {
                // Check if the slope is straight up(Vector3.up), if not: true.
                if (slopeHit.normal != Vector3.up)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }


        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            MyInput();
            ControlDrag();
            ControlSpeed();

            if (Input.GetKeyDown(jumpKey) && isGrounded)
            {
                // Jump.
                Jump();
            }

            slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        }

        void MyInput()
        {
            horizontalMovement = Input.GetAxisRaw("Horizontal");
            verticalMovement = Input.GetAxisRaw("Vertical");

            // Move in the direction the player object is facing.
            moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        }

        void Jump()
        {
            if (isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }

        void ControlSpeed()
        {
            if (Input.GetKey(sprintKey) && isGrounded)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
            }
        }

        void ControlDrag()
        {
            if (isGrounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = airDrag;
            }
        }

        private void FixedUpdate()
        {
            //Calling Movement() in FixedUpdate() has a frequency of a physic based and playerobject is using rigidbody so the movemnt look smooth.
            MovePlayer();
        }

        void MovePlayer()
        {
            if (isGrounded && !OnSlope())
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
            else if (isGrounded && OnSlope())
            {
                rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
            else if (!isGrounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
            }
        }
    }
}
