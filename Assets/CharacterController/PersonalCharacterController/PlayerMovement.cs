using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Transform playerObj;
        [SerializeField] Transform orientation;

        [Header("Input")]
        [SerializeField] private float sensX; 
        [SerializeField] private float sensY;
        private float mouseX;
        private float mouseY;
        private float xRotation;
        private float yRotation;

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

            //Control the y axis cause player cam wants to look horizontally (Left & Right).
            yRotation += mouseX;

            //Control the x axis cause player cam wants to look vertically (Up & Down).
            xRotation -= mouseY;

            //Clamp to prevent overrotation.
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            // Apply rotation on the camera. Allowing it to move Left & Right [yRotation] and Up & Down [xRotation].
            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

            // Apply rotation on the PlayerObj. Allowing it to move Left & Right [yRotation].
            playerObj.rotation = Quaternion.Euler(0f, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        private void InputManagement()
        {
            //Gather input from the mouse for camera movement.
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
    }
}
