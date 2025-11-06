using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl2
{
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Reference from Youtube Videos: [Ash Dev)]
        /// FPS controller tutorial in Unity | Part 01
        /// </summary>

        [Header("Reference")]
        private CharacterController characterController;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        private float xRotation;

        [Header("Movement Setting")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeedMultiplier = 2f;
        [SerializeField] private float sprintTransitSpeed = 5f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float jumpHeight = 2f;

        private float verticalVelocity;
        private float currentSpeed;
        private float currentSpeedMultiplier;

        [Header("Camera Bob")]
        [SerializeField] private float bobFrequency = 1f;
        [SerializeField] private float bobAmplitude = 1f;

        private CinemachineBasicMultiChannelPerlin noiseComponent;

        [Header("Input")]
        [SerializeField] private float mouseSensitivity;
        private float moveInput;
        private float turnInput;
        private float mouseX;
        private float mouseY;

        // Start is called before the first frame update
        void Start()
        {
            characterController = GetComponent<CharacterController>();

            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // Update is called once per frame
        void Update()
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
            // Create a movement vector based on player input.
            Vector3 move = new Vector3(turnInput, 0, moveInput);

            // Makes movement relative to the camera's facing direction.
            move = virtualCamera.transform.TransformDirection(move);

            // Apply sprint speed multiplier if holding Left Shift.
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeedMultiplier = sprintSpeedMultiplier;
            }
            else
            {
                currentSpeedMultiplier = 1f;
            }

            // Smoothly adjust current speed towards the target speed.
            currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);

            move *= currentSpeed;

            // Add vertical movement forces (e.g., gravity or jump).
            move.y = VerticalForceCalculation();

            // Move the character controller.
            characterController.Move(move * Time.deltaTime);
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
            if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
            {
                noiseComponent.m_FrequencyGain = bobFrequency * currentSpeedMultiplier;
                noiseComponent.m_AmplitudeGain = bobAmplitude * currentSpeedMultiplier;
            }
            //Not Grounded, No Shake.
            else
            {
                noiseComponent.m_FrequencyGain = 0.0f;
                noiseComponent.m_AmplitudeGain = 0.0f;
            }
        }

        private float VerticalForceCalculation()
        {
            if (characterController.isGrounded)
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
}
