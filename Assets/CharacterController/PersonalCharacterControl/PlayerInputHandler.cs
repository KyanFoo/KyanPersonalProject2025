using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;
    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    [HideInInspector] public bool jumpPressed;

    // Update is called once per frame
    void Update()
    {
        InputManagement();
    }

    public void InputManagement()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        jumpPressed = Input.GetKey(jumpKey);
    }
}
