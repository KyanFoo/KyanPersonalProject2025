using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform player;

    [Header("Camera Movement Setting")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        //Lock and remove Cursor from screen.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }
    private void Movement()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        // Get mouse input.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;

        //Prevent overrotation.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate the Virtual Camera.
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        //Rotating the Player.
        player.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
