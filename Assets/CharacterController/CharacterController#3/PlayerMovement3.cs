using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController3
{
    public class PlayerMovement3 : MonoBehaviour
    {
        public CharacterController controller;

        public float speed = 12f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;

        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        Vector3 velocity;
        public bool isGrounded;

        // Update is called once per frame.
        void Update()
        {
            // Create a SphereCollider to the "groundCheck" position.
            // Check for the "groundMask" layer using the "groundDistance" distance.
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            // Reset Player's velocity to 0 when grounded.
            if (isGrounded && velocity.y < 0)
            {
                // Set the velovity to "-2f" instead of "0f".
                // Is to force the plyaer to the ground incase the player is not totally on the ground.
                velocity.y = -2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);

            // Input "Space" to jump.
            if (Input.GetKeyDown("space") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity Control to allow the Player to fall.
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
