using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerController2;

public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform player;

    [Header("Camera Movement Setting")] 
    [SerializeField] private float sensX; // Mouse sensitivity on X-axis (horizontal).
    [SerializeField] private float sensY; // Mouse sensitivity on Y-axis (vertical).

    float xRotation; // Tracks vertical camera rotation (up/down).
    float yRotation; // Tracks horizontal camera rotation (left/right).

    [Header("Movement Setting")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag = 5f; //Lower = Less Friction, Higher = More Friction.
    float moveSpeed;
    float horizontalInput;
    float verticalInput;

    Vector3 move;

    public float currentSpeed;

    public MovementState state;

    [Header("Jump Setting")]
    [SerializeField] private float gravity = 30f;
    [SerializeField] private float jumpHeight = 2f;
    float verticalVelocity;

    public float airMultiplier;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    Vector3 slopeMoveDirection;

    [Header("Variable Check")]
    public Vector3 velocity;
    public float speed;
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
        ///////// Old Method (Raycast)
        //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        /////////

        // Handle Drag (Have some friction so that the character is not moving too fast).
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }

        Movement();
        SpeedControl();
        StateHandler();

        slopeMoveDirection = Vector3.ProjectOnPlane(move, slopeHit.normal).normalized;
        velocity = rb.velocity;
        speed = rb.velocity.magnitude;
    }
    private void FixedUpdate()
    {
        // Calculate extra gravity needed
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        GroundMovement();
    }
    // Handles all movement-related logic (currently only camera movement).
    private void Movement()
    {
        CameraMovement();
    }

    private void GroundMovement()
    {
        // Get keyboard Input.
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Movement direction relative to camera
        move = new Vector3(horizontalInput, 0, verticalInput);
        move = virtualCamera.transform.TransformDirection(move);
        move = move = move.normalized;

        move.y = VerticalForceCalculation();

        // Add force to the player.
        rb.AddForce(move * moveSpeed * 10f, ForceMode.Force);

        if (isGrounded)
        {
            rb.AddForce(move.normalized * moveSpeed * 10f, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * 10f, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(move.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Acceleration);
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

    private void SpeedControl()
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
    private float VerticalForceCalculation()
    {
        if (Input.GetKey(jumpKey) && isGrounded)
        {
            // Reset Y velocity to ensure consistent jump behavior
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            float Gravity = -(gravity);

            // Calculate required velocity to reach desired jump height
            float jumpVelocity = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Gravity));

            // Apply upward impulse force
            rb.AddForce(Vector3.up * jumpVelocity, ForceMode.Impulse);
        }
        return verticalVelocity;
    }


    // Handles camera and player rotation based on mouse input.
    private void CameraMovement()
    {
        // Get mouse movement input.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

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
