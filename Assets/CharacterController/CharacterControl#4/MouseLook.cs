using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl4
{
    public class MouseLook : MonoBehaviour
    {
        public Transform smoothCam;
        public float mouseSens;

        private float verticalRotation;
        private float horizontalRotation;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        void Update()
        {
            float verticalMouseMovement = Input.GetAxis("Mouse Y") * mouseSens;
            float horizontalMouseMovement = Input.GetAxis("Mouse X") * mouseSens;

            verticalMouseMovement *= -1; // The mouse Y axis is inverted.

            verticalRotation = Mathf.Clamp(verticalRotation + verticalMouseMovement, -90, 90);
            horizontalRotation += horizontalMouseMovement;

            transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, transform.eulerAngles.z);
            smoothCam.rotation = transform.rotation;
        }
        public Vector3 GetHorizontalRotation()
        {
            return Vector3.up * horizontalRotation;
        }
    }
}
