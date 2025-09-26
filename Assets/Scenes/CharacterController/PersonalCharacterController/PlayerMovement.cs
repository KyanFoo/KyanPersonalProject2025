using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform playerBody;
    public Transform orientation;

    [Header("Input Settings")]
    public float sensX = 150f;
    public float sensY = 150f;
    public float multiplier = 1f;

    private float mouseX;
    private float mouseY;
    private float verticalInput;
    private float horizontalInput;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Make the cursor at the centre of the screen.
        Cursor.visible = false;                     // Make the cursor not visible.

    }
    public void Update()
    {
        InputManagement();
        CameraMovement();
    }

    private void CameraMovement()
    {
        //Adjust mouse Sensitivity.
        mouseX *= sensX * Time.deltaTime * multiplier;   //Look horizontally (Left & Right).
        mouseY *= sensY * Time.deltaTime * multiplier;   //Look vertically (Up & Down).

        yRotation += mouseX;
        xRotation -= mouseY;

        //Clamp, to prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        // Apply rotation to camera.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        //Rotating the Player.
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void InputManagement()
    {
        // Gather inputs from the WASD keys for movement.
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");
    }

}
