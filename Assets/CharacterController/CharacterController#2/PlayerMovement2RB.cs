using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController2
{
    public class PlayerMovement2RB : MonoBehaviour
    {
        private Vector3 playerMovementInput;
        private Vector2 playerMouseInput;
        private float xRot;

        [SerializeField] private LayerMask floorMask;
        [SerializeField] private Transform feetTransform;
        [SerializeField] private Transform playerCam;
        [SerializeField] private Rigidbody playerBody;
        [Space]
        [SerializeField] private float speed;
        [SerializeField] private float sensitivity;
        [SerializeField] private float jumpForce;

        private void Update()
        {
            playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            MovePlayer();
            MovePlayerCam();
        }

        private void MovePlayer()
        {
            Vector3 MoveVector = transform.TransformDirection(playerMovementInput) * speed;
            playerBody.velocity = new Vector3(MoveVector.x, playerBody.velocity.y, MoveVector.z);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Physics.CheckSphere(feetTransform.position, 0.1f, floorMask))
                {
                    playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
        }

        private void MovePlayerCam()
        {
            xRot -= playerMouseInput.y * sensitivity;

            transform.Rotate(0f, playerMouseInput.x * sensitivity, 0f);
            playerCam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        }
    }
}
