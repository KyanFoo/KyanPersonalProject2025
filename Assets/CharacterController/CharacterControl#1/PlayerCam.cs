using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl1
{
    public class PlayerCam : MonoBehaviour
    {
        public float sensX;
        public float sensY;

        public Transform orientation;

        float xRotation;
        float yRotation;

        // Start is called before the first frame update.
        void Start()
        {
            // Make the cursor at the centre of the screen.
            // Make the cursor not visible.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        // Update is called once per frame.
        void Update()
        {
            // Get [mouse input].
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;

            xRotation -= mouseY;

            // Clamp the rotaion to ensure that the [cam] can look up and down more than 90 degree.
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate [cam] and [orientation].
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}
