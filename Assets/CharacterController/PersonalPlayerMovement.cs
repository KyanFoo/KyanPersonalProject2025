using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PersonalPlayerMovement : MonoBehaviour
{
    // ────────────────────────────────────────────────
    private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f; // Small vertical offset to improve ground check accuracy

    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Main virtual camera for look control
    [SerializeField] private Transform playerBody;                   // Reference for rotating the player's body
    [SerializeField] private Rigidbody playerRigidbody;              // Rigidbody used for physics movement
    [SerializeField] private CapsuleCollider playerCollider;         // Used for ground checks and character shape
    [SerializeField] private Transform orientation;                  // Forward/right direction reference for input

    // ────────────────────────────────────────────────
    [Header("Input Settings")]
    [SerializeField] private float sensX; // Mouse sensitivity for horizontal look
    [SerializeField] private float sensY; // Mouse sensitivity for vertical look

    private float verticalInput;          // Input from W/S or Up/Down
    private float horizontalInput;        // Input from A/D or Left/Right
    private float mouseX, mouseY;         // Mouse movement values
    private float xRotation, yRotation;   // Rotation values for camera and body
    private Vector3 finalDir;             // Final movement direction after processing

    // ────────────────────────────────────────────────
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 7f;               // Speed while walking
    [SerializeField] private float sprintSpeed = 10f;             // Speed while sprinting
    [SerializeField] private float airControlMultiplier = 0.4f;  // Movement control reduction in air
    [SerializeField] private float velocityMagnitude;            // Exposed: horizontal velocity magnitude (for UI/debug)

    private float moveSpeed;              // Current applied move speed
    private float minVelocity = 0.1f;     // Minimum threshold to apply velocity zeroing

    public enum MovementState { walking, sprinting, air }  // Movement state enum
    [SerializeField] private MovementState state;          // Current movement state

    // ────────────────────────────────────────────────
    [Header("Drag Settings")]
    [SerializeField] private float groundDrag = 6f; // Drag when grounded
    [SerializeField] private float airDrag = 2f;    // Drag when in the air

    // ────────────────────────────────────────────────
    [Header("Gravity Control Settings")]
    [SerializeField] private float gravityMultiplier = 1f;             // Normal gravity multiplier
    [SerializeField] private float extraGravity = 2f;                  // Extra downward force to fight floatiness
    [SerializeField] private float extraGravityTimeAfterSlope = 0.3f;  // Grace period after leaving a slope

    // ────────────────────────────────────────────────
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;  // Jumping power
    [SerializeField] private int maxJumps = 1;      // Max jump count (e.g. 2 = double jump)

    private int jumpsLeft;         // Remaining jumps available
    public bool pressedJump;      // Input buffer for jump key press

    // ────────────────────────────────────────────────
    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundMask;                 // Layer mask used to define ground
    [SerializeField] private float groundCheckDistance = 0.05f;    // Ground detection distance

    public bool isGrounded; // Whether the player is currently grounded

    // ────────────────────────────────────────────────
    [Header("Slope Handling Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;   // Max slope angle considered walkable
    [SerializeField] private float slideForce = 5f;       // Force applied when sliding down a steep slope

    public bool isOnSlope;         // If player is currently on a slope
    public bool isSlopeSteep;      // If slope is steeper than maxSlopeAngle
    public float slopeAngle;       // Measured angle of current slope
    private RaycastHit slopeHit;   // Stores slope raycast hit info
    private float lastTimeOnSlope; // Used for slope exit gravity handling

    // ────────────────────────────────────────────────
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;         // Jump key
    public KeyCode sprintKey = KeyCode.LeftShift;   // Sprint key

    // ────────────────────────────────────────────────
    [Header("DebugDraw Settings")]
    public bool debug = false;            // Enable debug drawing
    public float drawSphereSize = 0.1f;   // Size for gizmo drawing

    // ────────────────────────────────────────────────
    private void Start()
    {
        jumpsLeft = maxJumps; // Initialize jump count
    }

    private void Update()
    {
        InputManagement();   // Capture input
        SpeedControl();      // Apply speed caps and friction
        Statehandler();      // Determine current movement state
        CameraMovement();    // Rotate camera and body


        if (Input.GetKeyDown(jumpKey))
        {
            pressedJump = true; // Buffer jump input
        }
        //-----Explaination-----//
        // [Input.GetKey(jumpKey)] returns true every frame you hold down the key
        // Causing Jump() Function gets called multiple times, and jumpsLeft-- runs each time

        // [Input.GetKeyDown(jumpKey)] only return true on the frame the key is first pressed, preventing multi-jump spam.
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();     // Check ground
        isOnSlope = OnSlope();         // Check if on slope
        isSlopeSteep = IsTooSteep();   // Check if slope is too steep

        if (isGrounded && slopeAngle >= 0 && isOnSlope)
        {
            lastTimeOnSlope = Time.time; // Record slope timestamp
        }

        if (isGrounded)
        {
            jumpsLeft = maxJumps; // Reset jump count when grounded
        }

        ExtraGravity(); // Apply gravity modifications

        if (pressedJump)
        {
            pressedJump = false;
            if (isGrounded || jumpsLeft > 0)
            {
                Jump(); // Perform jump if allowed
            }
        }

        GroundMovement(); // Apply movement force
        ControlDrag();    // Apply proper drag
    }

    private void Statehandler()
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

    private void GroundMovement()
    {
        Vector3 dir = Vector3.zero;
        dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        finalDir = dir.normalized;

        if (debug)
        {
            Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
        }

        if (isGrounded && isSlopeSteep)
        {
            // Slide when on too steep a slope
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            playerRigidbody.AddForce(slideDir * slideForce, ForceMode.Force);
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
        }
        else if (isGrounded && isOnSlope)
        {
            // Move along slope
            playerRigidbody.AddForce(GetSlopeMoveDirection(dir) * moveSpeed * 10f, ForceMode.Force);
        }
        else if (isGrounded)
        {
            // Normal flat movement
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            // Airborne movement
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f * airControlMultiplier, ForceMode.Force);
        }
    }

    private void ExtraGravity()
    {
        // Always apply base gravity
        playerRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        // Apply stronger gravity shortly after leaving slope
        if (!pressedJump && !(isGrounded && slopeAngle <= maxSlopeAngle) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
        {
            Debug.Log("Extra");
            playerRigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        // Cancel current vertical velocity
        Vector3 velocity = playerRigidbody.velocity;
        velocity.y = 0;
        playerRigidbody.velocity = velocity;

        // Add upward jump force
        playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);

        if (!isGrounded)
        {
            jumpsLeft--; // Reduce available jumps if mid-air
        }
    }

    private void ControlDrag()
    {
        if (isGrounded)
        {
            // Apply higher drag to help stop quickly on the ground
            playerRigidbody.drag = groundDrag;
        }
        else
        {
            // Apply lower drag in the air to allow smoother falling and air movement
            playerRigidbody.drag = airDrag;
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

        // Limit velocity to max moveSpeed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
        }

        velocityMagnitude = flatVel.magnitude; // optional: for debugging or UI

        // Apply friction when idle on ground
        if (finalDir == Vector3.zero && isGrounded)
        {
            if (playerRigidbody.velocity.magnitude < minVelocity)
            {
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                // Apply friction opposite to velocity
                Vector3 frictionForce = -playerRigidbody.velocity.normalized * groundDrag;
                playerRigidbody.AddForce(frictionForce);
            }
        }
    }

    private void CameraMovement()
    {
        //Adjust mouse Sensitivity.
        mouseX *= sensX * Time.deltaTime;
        mouseY *= sensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        //Prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        //Rotate the Camera.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        //Rotate the Player.
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        //-----Explaination-----//
        //Set the player's rotation directly
        //Here’s how you can modify your script to unify both camera and player rotations using the same variables (xRotation and yRotation)
    }

    private Vector3 FeetPosition()
    {
        Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
        //-----Explaination-----//
        // (height / 2 - radius) gives you the distance from center to bottom of the flat capsule base, before the rounded part.
        // (Multiply by -1 * transform.up) pushes the offset downward in world space(i.e., towards the feet), regardless of player rotation.
        // [transform.up = Depends direction depends on the value. [+1 -> Up], [0 -> Centre], [-1 -> Down]

        Vector3 feetPosition = playerRigidbody.position + sphereOffset;
        // [playerRigidbody.position = Centre of playerRigidbody]

        return feetPosition;
    }

    private bool IsGrounded()
    {
        // Calculate the starting point of the SphereCast just above the feet to avoid clipping into the floor
        Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;

        // Define the length of the SphereCast including the offset
        float checkDistance = groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET;

        // True if grounded; otherwise, false
        return Physics.SphereCast(checkOrigin, playerCollider.radius * transform.localScale.x, -transform.up, out RaycastHit _, checkDistance, groundMask);
    }

    private bool OnSlope()
    {

        // Get the origin point for the slope check (bottom of capsule)
        Vector3 origin = FeetPosition();

        float distance = playerCollider.height * 0.5f * transform.localScale.y + 0.3f;

        // Perform a raycast straight downward to detect the surface below the player
        if (Physics.Raycast(origin, Vector3.down, out slopeHit, distance))
        {
            // Calculate the angle between the hit normal and world up (i.e., how steep the surface is)
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

            return slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
        }

        // If nothing was hit, reset slope angle and return false
        slopeAngle = 0f;

        // True if on a slope and within acceptable angle; otherwise, false
        return false;
    }

    private bool IsTooSteep()
    {
        // True if too steep to walk on; otherwise, false
        return slopeAngle > maxSlopeAngle;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 moveDir)
    {
        // Projects movement direction onto the slope plane using the surface normal
        // This ensures the player moves along the surface and not into it
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    public void OnDrawGizmosSelected()
    {
        if (debug)
        {
            //[Debug Draw the Feet Positon of the Sphere in the PlayerObj.]
            // ────────────────────────────────────────────────────────────

            //Gizmos.color = Color.green;
            //Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            //Vector3 feetPosition = playerRigidbody.position + sphereOffset;

            //Gizmos.DrawSphere(feetPosition, drawSphereSize);
            //Gizmos.DrawWireSphere(feetPosition, playerCollider.radius * transform.localScale.x);

            //[Debug Draws of IsGrounded() Collidiers]
            // ───────────────────────────────────────

            //Gizmos.color = Color.green;

            //Vector3 feetOffset = -1 * transform.up * groundCheckDistance;
            //Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
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
    }
}
