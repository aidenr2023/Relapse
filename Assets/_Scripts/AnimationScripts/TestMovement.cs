using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public float speed = 5f; // Movement speed

    private Rigidbody rb; // Declare a reference to Rigidbody

    void Start()
    {
        // Get the Rigidbody from the EnvironmentInteractionContext component
        EnvironmentInteractionContext context = FindObjectOfType<EnvironmentInteractionContext>();
        if (context != null)
        {
            rb = context.Rb;  // Access the Rigidbody property from EnvironmentInteractionContext
            Debug.Log("Rigidbody successfully assigned.");
        }
    }

    void FixedUpdate()
    {
   
        // Get input from the vertical and horizontal axes
        float moveVertical = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys
        
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys

        // Combine the movement directions
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);

        // Apply the movement using Rigidbody's velocity
        rb.velocity = movement * speed;  // Adjust 'speed' as necessary
    
    }
}
                    

