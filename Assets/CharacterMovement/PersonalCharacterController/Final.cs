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

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    float mouseX;
    float mouseY;
    float xRotation; // Tracks vertical camera rotation (up/down).
    float yRotation; // Tracks horizontal camera rotation (left/right).

    [Header("Movement Setting")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float movementMultiplier = 10f;
    [SerializeField] private float airMultiplier = 0.4f;
    float moveSpeed;

    [Header("Drag Setting")]
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 0.4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool isGrounded;

    [Header("Jump Setting")]
    [SerializeField] private float gravity = 30f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private bool readyToJump = true;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
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
        // Ground Check/
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        InputManagement();
        Movement();

        SpeedControl();
        StateHandler();

        // Handle Drag (Have some friction so that the character is not moving too fast).
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;


    }
    private void FixedUpdate()
    {
        // Calculate extra gravity needed
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        GroundMovement();
    }
    private void Movement()
    {
        CameraMovement();
    }
    private void GroundMovement()
    {
        // Movement direction relative to camera
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        //moveDirection = virtualCamera.transform.TransformDirection(moveDirection);
        moveDirection = orientation.TransformDirection(moveDirection);

        if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * 20f, ForceMode.Acceleration);
        }
        else if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            // Add force to the player.
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

        // Reset Y Velocity.
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

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

        // Update rotation values.
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
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // When to Jump.
        if ((Input.GetKey(jumpKey) && readyToJump && isGrounded))
        {
            readyToJump = false;

            JumpMovement();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private bool OnSlope()
    {
        // Slope Check/
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
}
