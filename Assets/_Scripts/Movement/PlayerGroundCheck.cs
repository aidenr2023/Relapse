using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallDetection : MonoBehaviour
{
    private Rigidbody rb;               // Reference to the Rigidbody component
    private bool isFalling = false;      // To check if the player is falling
    private float fallTime = 0f;         // Tracks how long the player has been falling
    private float fallThreshold = 0.5f;  // The time threshold for triggering the camera shake

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if the player is falling (negative Y velocity)
        if (rb.velocity.y < 0)
        {
            // If the player is falling, start counting the fall time
            if (!isFalling)
            {
                isFalling = true;
                fallTime = 0f; // Reset the fall timer
            }
            
            // Increment fall time
            fallTime += Time.deltaTime;
        }
        else
        {
            // If the player is not falling, reset the fall state
            if (isFalling)
            {
                isFalling = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player has landed on something (anything counts as ground)
        if (isFalling)
        {
            // If the player has been falling for more than the threshold, trigger the camera shake
            if (fallTime > fallThreshold)
            {
                // Call the camera shake method with intensity 5f and duration 0.1f
                CinemachineShake.Instance.ShakeCamera(10f, 0.1f);
                Debug.Log("Camera shake");
            }
            
            // Reset fall-related variables
            isFalling = false;
            fallTime = 0f;
        }
    }
}
