using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MovementState state;

    [Header("Camera Movement Setting")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    float xRotation;
    float yRotation;

    [Header("Movement Setting")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float targetGravity = -30f;
    float horizontalInput;
    float verticalInput;

    public float currentSpeedMultiplier;
    public float currentSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool grounded;

    private enum MovementState
    {
        walking,
        sprinting,
        air
    }
    // Start is called before the first frame update
    void Start()
    {
        //Lock and remove Cursor from screen.
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
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // Handle Drag (Have some friction so that the character is not moving too fast).
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }

        Movement();
    }
    private void FixedUpdate()
    {
        // Calculate extra gravity needed
        float extraGravity = targetGravity - Physics.gravity.y;
        rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);

        GroundMovement();
    }
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
        Vector3 move = new Vector3(horizontalInput, 0, verticalInput);
        move = virtualCamera.transform.TransformDirection(move);

        // Change state of Player.
        StateHandler();

        // Smooth transition between walk and sprint speed
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);
        move *= currentSpeed;

        // Apply force to Rigidbody
        rb.AddForce(move * moveSpeed * 5f, ForceMode.Force);
    }
    public void StateHandler()
    {
        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            currentSpeedMultiplier = sprintSpeedMultiplier;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            currentSpeedMultiplier = 1f;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void CameraMovement()
    {
        // Get mouse input.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;

        //Prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate the Virtual Camera.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        //Rotating the Player.
        player.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
