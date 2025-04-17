using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody body;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    [Header("Zoom")]
    public CinemachineFreeLook freeLookCam;
    private CinemachineFreeLook.Orbit[] originalOrbits;

    [Range(0.01f, 0.5f)]
    public float minZoom = 0.5f;
    [Range(1f, 5f)]
    public float maxZoom = 1.0f;
    public float zoomSens;
    public float zoomDampTime = 0.2f;
    float currentZoom = 1f;
    float targetZoom = 1f;
    private float zoomVelocity;

    [AxisStateProperty]
    public AxisState zAxis = new AxisState(0.5f, 5f, false, false, 1f, 0.1f, 0.1f, "Mouse ScrollWheel", false);

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat,
        Topdown,
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (freeLookCam != null)
        {
            originalOrbits = new CinemachineFreeLook.Orbit[freeLookCam.m_Orbits.Length];
            for (int i = 0; i < originalOrbits.Length; i++)
            {
                originalOrbits[i].m_Height = freeLookCam.m_Orbits[i].m_Height;
                originalOrbits[i].m_Radius = freeLookCam.m_Orbits[i].m_Radius;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Switch Camera Style.
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Combat);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.Topdown);


        // Rotate Orientation.
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // Rotate Player Object.
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }

        if (originalOrbits != null)
        {
            float zoomValue = Input.GetAxis("Mouse ScrollWheel") * zoomSens;
            if (Mathf.Abs(zoomValue) > 0.01f)
            {
                targetZoom -= zoomValue * zoomSens;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

            // Smooth damp zoom
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomDampTime);

            for (int i = 0; i < originalOrbits.Length; i++)
            {
                freeLookCam.m_Orbits[i].m_Height = originalOrbits[i].m_Height * currentZoom;
                freeLookCam.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * currentZoom;
            }
        }
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);

        currentStyle = newStyle;
    }
}
