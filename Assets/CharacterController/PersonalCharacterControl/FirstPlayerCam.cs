using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace KyanPersonalProject2025.PersonalCharacterController
{
    public class FirstPlayerCam : MonoBehaviour
    {
        [Header("References")]
        public Transform playerMesh;                    // Child object with mesh visuals
        public CinemachineVirtualCamera virtualCamera;  // Camera (Cinemachine or normal)
        public PlayerInputHandler playerInputHandler;
        public PlayerMovement playerMovement;

        [Header("Camera Settings")]
        public float sensX = 200f;
        public float sensY = 200f;

        private float xRotation = 0f;
        private float yRotation = 0f;

        void Update()
        {
            if (!playerMovement.canMove)
            {
                return; // Wait until spawn animation or ground contact
            }

            // --- CAMERA ROTATION ---
            float mouseX = playerInputHandler.mouseX * sensX * Time.deltaTime;
            float mouseY = playerInputHandler.mouseY * sensX * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Both have [yRotation] is to ensure that [Camera] and [PlayerMesh] is facing the same rotation, ensure that they are in sync.
            virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            playerMesh.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}
