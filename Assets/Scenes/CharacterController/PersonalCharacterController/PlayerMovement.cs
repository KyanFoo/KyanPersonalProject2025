using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Transform playerBody;

        [Header("Input Settings")]
        [SerializeField] private float sensX; 
        [SerializeField] private float sensY;

        private float mouseX;
        private float mouseY;
        private float verticalInput;
        private float horizontalInput;

        private float xRotation;
        private float yRotation;

        private void Update()
        {
            InputManagement();
            CameraMovement();
        }

        private void CameraMovement()
        {
            mouseX *= sensX * Time.deltaTime;
            mouseY *= sensY * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90, 90);

            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private void InputManagement()
        {
            // Gather inputs from the WASD keys for movement.
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            //Gather input from the mouse for camera movement.
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
    }
}
