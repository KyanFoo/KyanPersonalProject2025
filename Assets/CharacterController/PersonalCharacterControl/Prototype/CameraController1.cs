using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.Prototype
{
    public class CameraController1 : MonoBehaviour
    {

        [Header("Reference")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera; // Main virtual camera for look control
        [SerializeField] private Transform playerBody;                   // Reference for rotating the player's body

        [Header("Input Settings")]
        [SerializeField] private float sensX; // Mouse sensitivity for horizontal look
        [SerializeField] private float sensY; // Mouse sensitivity for vertical look

        private float verticalInput;          // Input from W/S or Up/Down
        private float horizontalInput;        // Input from A/D or Left/Right
        private float mouseX, mouseY;         // Mouse movement values
        private float xRotation, yRotation;   // Rotation values for camera and body

        private void Update()
        {
            InputManagement();
            CameraMovement();
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
}
