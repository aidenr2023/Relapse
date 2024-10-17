using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHandler : MonoBehaviour
{
    public Transform oppositeTransform1; // First opposite transform
    public Transform oppositeTransform2; // Second opposite transform
    public Transform currentTargetTransform; // The current target transform to switch to

    public Rigidbody rb; // Reference to the gun's Rigidbody

    private void Start()
    {
        // Initialize the current target transform
        currentTargetTransform = oppositeTransform1;

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody component is missing from the gun.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is on the "Wall" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Switch the gun's transform to the current target transform
            SwitchToOppositeTransform();

            // Toggle the target transform to switch back and forth
            currentTargetTransform = (currentTargetTransform == oppositeTransform1) ? oppositeTransform2 : oppositeTransform1;
        }
    }

    private void SwitchToOppositeTransform()
    {
        // Ensure the current target transform is assigned
        if (currentTargetTransform != null)
        {
            // Set the gun's position and rotation to the current target transform
            transform.position = currentTargetTransform.position;
            transform.rotation = currentTargetTransform.rotation;

            // Reset the velocity to avoid physics glitches
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            Debug.LogWarning("Target Transform is not assigned.");
        }
    }
}
