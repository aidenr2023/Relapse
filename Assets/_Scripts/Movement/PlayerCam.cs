using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // Include the new Input System namespace

public class PlayerCam : MonoBehaviour
{
    public float sensX;  // Sensitivity on the X axis
    public float sensY;  // Sensitivity on the Y axis
    
    public PlayerMovement pm;  // Reference to PlayerMovement script
    public WallRunning wr;     // Reference to WallRunning script
    public Transform orientation;  // Reference to the player's orientation transform

    private float xRotation;  // Current rotation around the X axis
    private float yRotation;  // Current rotation around the Y axis

    [Header("Input Actions")]
    public InputActionReference lookAction;  // Input action for looking around

    private void Start() 
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lookAction.action.Enable();  // Enable the input action for looking around
    }

    private void OnDestroy()
    {
        // Disable the input action when the script is destroyed
        lookAction.action.Disable();  
    }

    private void Update() 
    {
        // Get mouse input from the new Input System
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        float mouseX = lookInput.x * Time.deltaTime * sensX;
        float mouseY = lookInput.y * Time.deltaTime * sensY;

        // Adjust rotation based on mouse input
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Clamp the X rotation to prevent over-rotation

        // Rotate the camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Adjust camera angle based on wall running state
        // if (pm.isWallRunning && wr.wallRight)
        // {
        //     transform.rotation = Quaternion.Euler(xRotation, yRotation, -25f);
        // }
        // else if (pm.isWallRunning && wr.wallLeft)
        // {
        //     transform.rotation = Quaternion.Euler(xRotation, yRotation, 25f);
        // }

        // Update the orientation to match the camera's forward direction
        orientation.transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
    }
}
