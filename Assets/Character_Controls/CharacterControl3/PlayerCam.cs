using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    [Header("Others")]
    public Transform orientation;
    public Transform playerBody;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        // Cause the Cursor to be locked in the middle of the screen.
        Cursor.lockState = CursorLockMode.Locked;

        // Cause the Cursor turns invisible.
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Get mouse Input.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate camera.
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Rotate orientation and player body.
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        playerBody.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
