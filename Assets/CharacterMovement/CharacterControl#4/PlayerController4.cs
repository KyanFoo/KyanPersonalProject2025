using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController4 : MonoBehaviour
{
    [Header("Reference")]
    private CharacterController controller;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Movement Setting")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float turningSpeed = 2f;
    // Mimic earth gravity.
    [SerializeField] private float gravity = 0f;
    [SerializeField] private float jumpHeight = 2f;

    // Keep track of the vertical movement speed of the player.
    private float verticalVelocity;
    private float currentSpeedMultiplier;
    private float currentSpeed;

    [Header("Camera Bob")]
    // How fast you want the shake to occur.
    [SerializeField] private float bobFrequency = 1f;
    // How strong you want the shake to be.
    [SerializeField] private float bobAmptitude = 1f;

    private CinemachineBasicMultiChannelPerlin noiseComponent;

    [Header("Input")]
    [SerializeField] private float mouseSensitivity;
    // Capture the player's front and back movement.
    private float moveInput;
    // Capture the player's turning movement.
    private float turnInput;

    // Store Mouse Inputs;
    private float mouseX;
    private float mouseY;

    private float xRotation;

    private void Start()
    {
        //Get Component form the player object.
        controller = GetComponent<CharacterController>();

        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        InputManagement();
        Movement();
    }

    private void LateUpdate()
    {
        CameraBob();
    }

    private void Movement()
    {
        GroundMovement();
        Turn();
    }

    private void GroundMovement()
    {
        Vector3 move = new Vector3(turnInput, 0, moveInput);
        move = virtualCamera.transform.TransformDirection(move);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);
        move *= currentSpeed;

        move.y = VerticalForceCalculation();

        controller.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        //Adjust mouse Sensitivity.
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        //Prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //Rotating the Player.
        transform.Rotate(Vector3.up * mouseX);
    }

    private void CameraBob()
    {
        //Grounded, Shake.
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            noiseComponent.m_FrequencyGain = bobFrequency * currentSpeedMultiplier;
            noiseComponent.m_AmplitudeGain = bobAmptitude * currentSpeedMultiplier;
        }
        //Not Grounded, No Shake.
        else
        {
            noiseComponent.m_FrequencyGain = bobFrequency;
            noiseComponent.m_AmplitudeGain = bobAmptitude;
        }
    }

    private float VerticalForceCalculation()
    {
        if (controller.isGrounded)
        {
            //Ensure that the plyaer maintain contact to the ground and doesn't start to float.
            verticalVelocity = -1f;

            if (Input.GetButtonDown("Jump"))
            {
                //Calculate the intial velocty needed to reach to the desire height, factoring in gravity pull.
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
            }
        }
        else
        {
            //Simulate falling.
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void InputManagement()
    {
        // Gather inputs from the WASD keys for movement.
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
