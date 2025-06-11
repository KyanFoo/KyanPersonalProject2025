using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PersonalPlayerMovement : MonoBehaviour
{
    public float minVelocity = 0.1f;
    public float velocityMagnitude;
    private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

    [Header("Reference")]
    [SerializeField] private MovementState state;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private Transform orientation;


    [Header("Input Settings")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    float mouseX;
    float mouseY;

    float xRotation;
    float yRotation;

    float verticalInput;
    float horizontalInput;

    Vector3 finalDir;

    [Header("Movement Settings")]
    float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] public bool isGrounded;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("DebugDraw Settings")]
    public bool debug;
    public float drawSphereSize;

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Update()
    {
        InputManagement();
        SpeedControl();
        Statehandler();

        CameraMovement();
    }
    private void FixedUpdate()
    {
        isGrounded = IsGrounded();

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
        dir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        finalDir = dir.normalized;
        if (isGrounded)
        {
            playerRigidbody.AddForce(finalDir * moveSpeed * 10f, ForceMode.Force);
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
        Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
        }

        velocityMagnitude = flatVel.magnitude; // optional: for debugging or UI

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

        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        //Rotating the Player.
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    private Vector3 FeetPosition()
    {
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
        Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
        bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);

        return isGrounded;
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
            /////////////////////////////////////////////////////////////
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
