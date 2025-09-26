using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

    [Header("Reference")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform playerBody;
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerCollider;
    public Transform orientation;

    [Header("Input Settings")]
    public float sensX = 150f;
    public float sensY = 150f;
    public float multiplier = 1f;

    private float mouseX;
    private float mouseY;
    private float verticalInput;
    private float horizontalInput;

    private float xRotation;
    private float yRotation;

    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.05f;

    public bool isGrounded;

    [Header("DebugDraw Settings")]
    public bool debug = false;

    [System.Flags]
    public enum DebugTesting
    {
        None = 0,
        Debug1_FeetPosition = 1 << 0,
        Debug2_CastIsGrounded = 1 << 1,
        Debug3_IsGrounded = 1 << 2,
        Debug4_LerpMotion = 1 << 3,
        Debug5 = 1 << 4,
        Debug6 = 1 << 5,
        Debug7 = 1 << 6,
    }
    public DebugTesting debugTypes;
    [Range(0f, 1f)][SerializeField] float debugCastProgress = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Make the cursor at the centre of the screen.
        Cursor.visible = false;                     // Make the cursor not visible.

    }
    public void Update()
    {
        InputManagement();
        CameraMovement();
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();      // Check ground.
    }

    private void CameraMovement()
    {
        //Adjust mouse Sensitivity.
        mouseX *= sensX * Time.deltaTime * multiplier;   //Look horizontally (Left & Right).
        mouseY *= sensY * Time.deltaTime * multiplier;   //Look vertically (Up & Down).

        yRotation += mouseX;
        xRotation -= mouseY;

        //Clamp, to prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        // Apply rotation to camera.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        //Rotating the Player.
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private Vector3 FeetPosition()  // --- Calculate feet position for raycasts ---
    {
        Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
        Vector3 feetPosition = playerRigidbody.position + sphereOffset;
        return feetPosition;
    }

    private bool IsGrounded()   // --- Grounded check using SphereCast ---
    {
        //Move the cast origin slightly upward from the feet to prevent clipping.
        Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;
        float checkDistance = groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET;
        return Physics.SphereCast(checkOrigin, playerCollider.radius * transform.localScale.x, -transform.up, out RaycastHit _, checkDistance, groundMask);
    }

    private void OnDrawGizmos() // --- Debug gizmos for visualizing ground check ---
    {
        if (debug)  // Only run if debug mode is enabled.
        {
            if (debugTypes.HasFlag(DebugTesting.Debug1_FeetPosition)) // Show the FeetPosition as a wire sphere.
            {
                //Draw a WireSphere to show the FeetPosition point.
                Gizmos.DrawWireSphere(FeetPosition(), playerCollider.radius * transform.localScale.x);
            }

            if (debugTypes.HasFlag(DebugTesting.Debug2_CastIsGrounded)) // Show the SphereCast origin (slightly above feet).
            {
                //Draw a WireSphere to show the IsGrounded point slightly above the FeetPositon to avoid clipping. ###
                Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(checkOrigin, playerCollider.radius * transform.localScale.x);
            }

            if (debugTypes.HasFlag(DebugTesting.Debug3_IsGrounded)) // Show the expected grounded check endpoint.
            {
                // Draw a WireSphere to visualize a FeetPosition Collidier for Player Prefab for Ground & Slope Check.
                Vector3 feetOffset = -1 * groundCheckDistance * transform.up;

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
            }

            if (debugTypes.HasFlag(DebugTesting.Debug4_LerpMotion)) // Visualize the entire SphereCast motion using interpolation.
            {
                // SphereCast start point (above feet to avoid clipping)
                Vector3 checkOrigin = FeetPosition() + transform.up * GROUND_CHECK_SPHERE_OFFSET;
                Vector3 feetOffset = -1 * groundCheckDistance * transform.up;

                // ----- Motion simulation -----
                // debugCastProgress = 0 > start, 1 > end
                Vector3 movingSpherePos = Vector3.Lerp(checkOrigin, FeetPosition() + feetOffset, debugCastProgress);
                Gizmos.color = Color.cyan; // moving sphere color
                Gizmos.DrawWireSphere(movingSpherePos, playerCollider.radius * transform.localScale.x);
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
    }

}
