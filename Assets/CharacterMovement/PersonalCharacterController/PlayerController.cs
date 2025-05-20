using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform player;

    [Header("Camera Movement Setting")] 
    [SerializeField] private float sensX; // Mouse sensitivity on X-axis (horizontal).
    [SerializeField] private float sensY; // Mouse sensitivity on Y-axis (vertical).

    float xRotation; // Tracks vertical camera rotation (up/down).
    float yRotation; // Tracks horizontal camera rotation (left/right).

    // Start is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    // Handles all movement-related logic (currently only camera movement).
    private void Movement()
    {
        CameraMovement();
    }

    // Handles camera and player rotation based on mouse input.
    private void CameraMovement()
    {
        // Get mouse movement input.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Update rotation values.
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp vertical rotation to prevent camera flipping over.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate the virtual camera on both vertical and horizontal axes.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Rotate the player to the direction the virtual camera is facing.
        player.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
