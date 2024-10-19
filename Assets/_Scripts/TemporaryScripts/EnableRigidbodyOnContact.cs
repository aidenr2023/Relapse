using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableRigidbodyOnContact : MonoBehaviour
{
    private Rigidbody rb;

    private void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();

        // Make sure the Rigidbody is disabled at the start
        if (rb != null)
        {
            rb.isKinematic = true; // or rb.useGravity = false; depending on your needs
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player")) // Make sure the player has the "Player" tag
        {
            EnableRigidbody();
        }
    }

    private void EnableRigidbody()
    {
        if (rb != null)
        {
            rb.isKinematic = false; // Enable the Rigidbody
            // Optionally, you can enable gravity as well
            rb.useGravity = true;
            // You can also apply an initial force or set velocity here if needed
        }
    }
}