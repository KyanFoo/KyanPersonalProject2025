using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetTester : MonoBehaviour
{
    [Header("References")]
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerCollider;

    [Header("Gizmo Settings")]
    public bool drawGizmos = true;
    public float gizmoSize = 0.05f;

    private void Reset()
    {
        // Auto-assign on add
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// Universal method to get the character's feet position.
    /// Works across different scales, colliders, and models.
    /// </summary>
    public Vector3 GetFeetPosition()
    {
        // Center of collider in world space
        Vector3 colliderCenter = playerCollider.bounds.center;

        // Half height of collider (scaled automatically by bounds)
        float halfHeight = playerCollider.bounds.extents.y;

        // Correctly scaled radius (handles non-uniform scales in X/Z)
        float scaledRadius = playerCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);

        // Calculate feet position
        Vector3 feetPos = colliderCenter - transform.up * (halfHeight - scaledRadius);

        return feetPos;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || playerCollider == null) return;

        Vector3 feetPos = GetFeetPosition();

        // Draw calculated FeetPosition
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(feetPos, gizmoSize);

        // For debugging: draw naive collider bottom for comparison
        Vector3 naiveBottom = playerCollider.bounds.center - Vector3.up * playerCollider.bounds.extents.y;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(naiveBottom, gizmoSize * 0.75f);

        // Line between center and feet
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCollider.bounds.center, feetPos);
    }
}
