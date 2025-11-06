using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.Prototype
{
    public class CameraController3 : MonoBehaviour
    {
        [Header("References")]
        public Transform playerMesh;       // Child object with mesh visuals
        public Transform virtualCamera;    // Camera (Cinemachine or normal)
        public Rigidbody rb;

        [Header("Sensitivity")]
        public float sensX = 200f;
        public float sensY = 200f;

        private float xRotation = 0f;
        private float yRotation = 0f;

        [Header("Keybinds")]
        public KeyCode resetKey = KeyCode.Space;

        void Update()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

            // Accumulate rotation
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate camera vertically
            virtualCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotate player mesh horizontally
            playerMesh.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            if (Input.GetKey(resetKey))
            {
                ResetPlayerState();
            }
        }

        public void ResetPlayerState()
        {
            transform.position = Vector3.zero; // Or rb.position = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
