using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Final : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform player;
    public MovementState state;

    [Header("Input Settings")]
    [SerializeField] private float sensX; // Mouse sensitivity on X-axis (horizontal).
    [SerializeField] private float sensY; // Mouse sensitivity on Y-axis (vertical).

    // Input values
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection; // Final direction player will move in

    float mouseX;
    float mouseY;
    float xRotation; // Tracks vertical camera rotation (up/down).
    float yRotation; // Tracks horizontal camera rotation (left/right).

    [Header("Movement Setting")]
    [SerializeField] private float walkSpeed; // Speed while walking.
    [SerializeField] private float sprintSpeed; // Speed while sprinting.
    [SerializeField] private float movementMultiplier = 10f;
    [SerializeField] private float airMultiplier = 0.4f;
    float moveSpeed; // Current move speed based on state

    [Header("Drag Setting")]
    [SerializeField] private float groundDrag = 5f; // Friction when grounded
    [SerializeField] private float airDrag = 0.4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround; // Define ground layer
    [SerializeField] private bool isGrounded; // Is the player currently grounded

    [Header("Jump Setting")]
    [SerializeField] private float gravity = 30f; // Base gravity
    [SerializeField] private float jumpForce; // Upward force on jump
    [SerializeField] private float jumpCooldown; // Cooldown time before jumping again
    [SerializeField] private bool readyToJump = true; // Can the player currently jump
    [SerializeField] private float gravityMultiplier = 1.5f; // Extra gravity when falling

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle; // Maximum angle that is still considered walkable
    RaycastHit slopeHit;
    Vector3 slopeMoveDirection;
    bool exitingSlope;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    // Start is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Assign Rigidbody & Freeze Rotation.
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if grounded using Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        InputManagement(); // Handle input
        Movement(); // Handle camera rotation

        SpeedControl(); // Limit movement speed
        StateHandler(); // Handle sprint/walk/air state

        // Handle Drag (Have some friction so that the character is not moving too fast).
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }

        // Calculate slope-adjusted movement direction
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void FixedUpdate()
    {
        // Apply base gravity
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        GroundMovement(); // Handle player movement based on slope and state

        // Apply extra gravity when falling
        if (!isGrounded && rb.velocity.y < 0)
        {
            float extraGravity = gravity * (gravityMultiplier - 1f);  // only add the extra portion
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    private void Movement()
    {
        CameraMovement(); // Handle camera rotation from mouse
    }

    private void GroundMovement()
    {
        // Movement direction relative to camera
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        moveDirection = orientation.TransformDirection(moveDirection); // Align movement with camera orientation

        if (isGrounded && OnSlope())
        {
            // Move up/down slope smoothly
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * 20f, ForceMode.Acceleration);
        }
        else if (isGrounded)
        {
            // Regular ground movement
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            // Aerial movement
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }

    public void StateHandler()
    {
        // Mode - Sprinting
        if (isGrounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (isGrounded)
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

    private void JumpMovement()
    {
        exitingSlope = true;

        // Reset Y Velocity to ensure consistent jump height.
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply upward force.
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private void CameraMovement()
    {
        //Adjust mouse Sensitivity.
        mouseX *= sensX * Time.deltaTime;
        mouseY *= sensY * Time.deltaTime;

        // Adjust rotation values.
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp vertical rotation to prevent camera flipping over.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate the virtual camera on both vertical and horizontal axes.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Rotate the player to the direction the virtual camera is facing.
        player.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void SpeedControl()
    {
        // Limiting speed on slope.
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        // Limiting speed on ground or in air.
        else if (isGrounded)
        {
            // gather the RigidBody's Velocity.
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Limit Velocity if needed.
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;

                // Corrected the RigidBody's Velocity to the moveSpeed.
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void InputManagement()
    {
        // Gather inputs from the WASD keys for movement.
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        // Jump input.
        if ((Input.GetKey(jumpKey) && readyToJump && isGrounded))
        {
            readyToJump = false;

            JumpMovement();

            Invoke(nameof(ResetJump), jumpCooldown); // Reset jump after cooldown.
        }
    }
    private bool OnSlope()
    {
        // Check for slope and compare angle to maxSlopeAngle
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
}
