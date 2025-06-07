using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PersonalPlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CapsuleCollider playerCollider;

    [Header("Input Settings")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    float mouseX;
    float mouseY;

    float xRotation;
    float yRotation;

    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] public bool isGrounded;

    [Header("DebugDraw Settings")]
    public bool debug;
    public float drawSphereSize;

    private void Update()
    {
        InputManagement();
        CameraMovement();
    }
    private void FixedUpdate()
    {
        isGrounded = IsGrounded();
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

        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        //Rotating the Player.
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    private Vector3 FeetPosition()
    {
        Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
        // (height / 2 - radius) gives you the distance from center to bottom of the flat capsule base, before the rounded part.

        // (Multiply by -1 * transform.up) pushes the offset downward in world space(i.e., towards the feet), regardless of player rotation.
        // [transform.up = Depends direction depends on the value. [+1 -> Up], [0 -> Centre], [-1 -> Down]

        Vector3 feetPosition = playerRigidbody.position + sphereOffset;
        // [playerRigidbody.position = Centre of playerRigidbody]

        return feetPosition;
    }

    private bool IsGrounded()
    {
        //bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset, playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);
        bool isGrounded = Physics.SphereCast(FeetPosition(), playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info, groundCheckDistance, groundMask);


        return isGrounded;
    }

    public void OnDrawGizmosSelected()
    {
        if (debug)
        {
            //[Debug Draw the Feet Positon of the Sphere in the PlayerObj.]
            //Gizmos.color = Color.green;
            //Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
            //Vector3 feetPosition = playerRigidbody.position + sphereOffset;

            //Gizmos.DrawSphere(feetPosition, drawSphereSize);
            //Gizmos.DrawWireSphere(feetPosition, playerCollider.radius * transform.localScale.x);
            /////////////////////////////////////////////////////////////
            //[Debug Draws of IsGrounded() Collidiers]
            //Gizmos.color = Color.green;

            //Vector3 feetOffset = -1 * transform.up * groundCheckDistance;
            //Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
        }
    }

    private void InputManagement()
    {
        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
