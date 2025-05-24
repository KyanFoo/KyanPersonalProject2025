using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityDebugger : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Print the magnitude of the velocity vector
        float speed = rb.velocity.magnitude;
        Debug.Log("Current Speed: " + speed.ToString("F2"));
    }
}
