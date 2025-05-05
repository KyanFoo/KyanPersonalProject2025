using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private PlayerControls playerControls;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        // Enable the playerControls.
        playerControls.Enable();
    }
    private void Disable()
    {
        // Disable the playerControls.
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        // Get [Movement] input.
        return playerControls.Player.Movement.ReadValue<Vector2>();
    }
    public Vector2 GetMouseDelta()
    {
        // Get [Mouse] input.
        return playerControls.Player.Look.ReadValue<Vector2>();
    }
    public bool PlayerJumpThisFrame()
    {
        // Get [Space] input.
        return playerControls.Player.Jump.triggered;
    }
}
