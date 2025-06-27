using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PersonalPlayerMovement : MonoBehaviour
{
    private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f; // Offset for SphereCast
    public float minVelocity = 0.1f; // Velocity below which friction is applied
    public float velocityMagnitude; // Magnitude of horizontal velocity

    [Header("Input Settings")]
    [SerializeField] private float sensX; // Mouse sensitivity X axis
    [SerializeField] private float sensY; // Mouse sensitivity Y axis

    private float mouseX, mouseY; // Mouse movement
    private float xRotation, yRotation; // Camera rotation
    private float verticalInput, horizontalInput; // WASD input
    private Vector3 finalDir; // Final calculated direction

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed; // Speed when walking
    [SerializeField] private float sprintSpeed; // Speed when sprinting
    [SerializeField] private float airControlMultiplier = 0.4f; // Air movement control multiplier

    private float moveSpeed; // Final current speed

    [Header("Drag Settings")]
    [SerializeField] private float groundDrag = 6f; // Drag when grounded
    [SerializeField] private float airDrag = 2f; // Drag when in air

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f; // Jump strength
    [SerializeField] private int maxJumps = 1; // Number of allowed jumps

    private int jumpsLeft; // Jump counter
    private bool pressedJump; // Buffer jump key press

    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundMask; // Layer to check as ground
    [SerializeField] private float groundCheckDistance = 0.05f; // Distance for ground check

    public bool isGrounded; // If player is touching ground

    [Header("Slope Handling Settings")]
    [SerializeField] private float maxSlopeAngle = 45f; // Max climbable slope angle
    [SerializeField] private float slideForce = 5f;
    public bool isOnSlope; // If player is on a slope
    public bool isSlopeSteep;
    public float slopeAngle; // Angle of the slope
    private RaycastHit slopeHit; // Stores hit info on slope
    private float lastTimeOnSlope;

    [Header("Gravity Control Settings")]
    [SerializeField] private float gravityMultiplier = 1f;
    [SerializeField] private float extraGravity = 2f;
    [SerializeField] private float extraGravityTimeAfterSlope = 0.3f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space; // Jump key
    public KeyCode sprintKey = KeyCode.LeftShift; // Sprint key

    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Camera reference
    [SerializeField] private Transform playerBody; // Player rotation transform
    [SerializeField] private Rigidbody playerRigidbody; // Rigidbody
    [SerializeField] private CapsuleCollider playerCollider; // Capsule collider
    [SerializeField] private Transform orientation; // Transform for direction reference
    public enum MovementState { walking, sprinting, air } // State machine for movement
    [SerializeField] private MovementState state; // Current state

    [Header("DebugDraw Settings")]
    public bool debug; // Toggle debug drawing
    public float drawSphereSize; // Size for drawn spheres

    private void Start()
    {
        jumpsLeft = maxJumps; // Initialize jump count
    }

    private void Update()
    {
        InputManagement();
        SpeedControl();
        Statehandler();
        CameraMovement();

        // Detect jump key pressed
        if (Input.GetKeyDown(jumpKey))
        {
            pressedJump = true;
        }
    }
    private void FixedUpdate()
    {
        isGrounded = IsGrounded();
        isOnSlope = OnSlope();
        isSlopeSteep = IsTooSteep();

        if (isGrounded && slopeAngle >= 0 && isOnSlope)
        {
            lastTimeOnSlope = Time.time;
        }

        // Reset jumps when grounded
        if (isGrounded)
        {
            jumpsLeft = maxJumps;
        }

        ExtraGravity();

        // Initiate Jump() Function
        if (pressedJump)
        {
            pressedJump = false;

            // Check to see if [isGrounded] is true and [jumpLeft] is more than 0
            if (isGrounded || jumpsLeft > 0)
            {
                Jump();
            }
        }

        GroundMovement();
        ControlDrag();
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

        // Calculate direction from input
        dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        finalDir = dir.normalized;

        // Apply force based on grounded or air
        if (isGrounded && isSlopeSteep)
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;

            playerRigidbody.AddForce(slideDir * slideForce, ForceMode.Force);
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);

        }
        else if (isGrounded && isOnSlope)
        {
            playerRigidbody.AddForce(GetSlopeMoveDirection(dir) * moveSpeed * 10f, ForceMode.Force);
            //Debug.Log("Slope");
        }
        else if (isGrounded)
        {
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
            //Debug.Log("Ground");
        }
        else
        {
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f * airControlMultiplier, ForceMode.Force);
        }
    }

    private void ExtraGravity()
    {
        // Add extra gravity in general
        playerRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        if (!pressedJump && !(isGrounded && isOnSlope) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
        {
            playerRigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        // Reset vertical velocity to prevent stacking
        Vector3 currentVelocity = playerRigidbody.velocity;
        currentVelocity.y = 0;
        playerRigidbody.velocity = currentVelocity;

        // Apply jump force
        playerRigidbody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);

        // Apply multiple jump force in the air
        if (!isGrounded)
        {
            jumpsLeft--;
        }
    }

    private void ControlDrag()
    {
        if (isGrounded)
        {
            playerRigidbody.drag = groundDrag;
        }
        else
        {
            playerRigidbody.drag = airDrag;
        }
    }

    private void SpeedControl()
    {
        // Limit velocity to max moveSpeed
        Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

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
    }
    private Vector3 FeetPosition()
    {
        // Function: Calculates bottom of capsule for ground check

        Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
        // (height / 2 - radius) gives you the distance from center to bottom of the flat capsule base, before the rounded part.

        // (Multiply by -1 * transform.up) pushes the offset downward in world space(i.e., towards the feet), regardless of player rotation.
        // [transform.up = Depends direction depends on the value. [+1 -> Up], [0 -> Centre], [-1 -> Down]

        Vector3 feetPosition = playerRigidbody.position + sphereOffset;
        // [playerRigidbody.position = Centre of playerRigidbody]

        return feetPosition;
    }

    private bool IsGrounded()
    {
        // Raycast check for ground
        Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
        bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);

        return isGrounded;
    }

    private bool OnSlope()
    {
        Vector3 origin = FeetPosition();
        float castDistance = playerCollider.height * 0.5f * transform.localScale.y + 0.3f;

        if (Physics.Raycast(origin, Vector3.down, out slopeHit, castDistance))
        {
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
        }

        slopeAngle = 0f;
        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 moveDir)
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    private bool IsTooSteep()
    {
        return slopeAngle > maxSlopeAngle;
    }

    public void OnDrawGizmosSelected()
    {
        if (debug)
        {
            //[Debug Draw the Feet Positon of the Sphere in the PlayerObj.]
            //Gizmos.color = Color.green;
            //Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            //Vector3 feetPosition = playerRigidbody.position + sphereOffset;

            //Gizmos.DrawSphere(feetPosition, drawSphereSize);
            //Gizmos.DrawWireSphere(feetPosition, playerCollider.radius * transform.localScale.x);


            //[Debug Draws of IsGrounded() Collidiers]
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
