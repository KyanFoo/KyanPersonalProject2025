using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PersonalPlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform playerBody;

    [Header("Input Settings")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    float mouseX;
    float mouseY;

    float xRotation;
    float yRotation;

    private void Update()
    {
        InputManagement();
        CameraMovement();
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

    private void InputManagement()
    {
        //Gather input from the mouse for camera movement.
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
