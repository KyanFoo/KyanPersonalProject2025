using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController2
{
    public class PlayerMovement2CC : MonoBehaviour
    {
        private Vector3 velocity;
        private Vector3 playerMovementInput;
        private Vector3 playerMouseInput;
        private float xRot;

        [SerializeField] private Transform playerCam;
        [SerializeField] private CharacterController controller;
        [Space]
        [SerializeField] private float speed;
        [SerializeField] private float sensitivity;
        [SerializeField] private float jumpForce;
        [SerializeField] private float gravity = -9.81f;

        private void Update()
        {
            playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            MovePlayer();
            MovePlayerCam();
        }

        private void MovePlayer()
        {
            Vector3 MoveVector = transform.TransformDirection(playerMovementInput);

            if (controller.isGrounded)
            {
                velocity.y = -1f;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y = jumpForce;
                }
            }
            else
            {
                velocity.y -= gravity * -2f * Time.deltaTime;
            }

            controller.Move(MoveVector * speed * Time.deltaTime);
            controller.Move(velocity * Time.deltaTime);

        }

        private void MovePlayerCam()
        {
            xRot -= playerMouseInput.y * sensitivity;

            transform.Rotate(0f, playerMouseInput.x * sensitivity, 0f);
            playerCam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        }
    }
}