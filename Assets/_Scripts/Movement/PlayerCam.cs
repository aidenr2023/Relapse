using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization; // Include the new Input System namespace

public class PlayerCam : MonoBehaviour
{
    [Header("Settings")]
    // Mouse sensitivity
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [Header("References")]
    // Reference to PlayerMovement script
    [SerializeField]
    private PlayerMovement playerMovement;

    // Reference to WallRunning script
    [SerializeField] private WallRunning wallRunning;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    private float _xRotation; // Current rotation around the X axis
    private float _yRotation; // Current rotation around the Y axis

    // [Header("Input Actions")]
    // public InputActionReference lookAction;  // Input action for looking around

    private void Start()
    {
        // // Lock the cursor to the center of the screen and make it invisible
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        // lookAction.action.Enable();  // Enable the input action for looking around

        // Initialize the input
        InitializeInput();
    }

    private void InitializeInput()
    {
        // Initialize the input
        InputManager.Instance.PlayerControls.GamePlay.Look.performed += OnLook;
    }

    private void OnLook(InputAction.CallbackContext obj)
    {
        var lookInput = obj.ReadValue<Vector2>();
        
        // Adjust rotation based on mouse input
        _yRotation += lookInput.x * Time.deltaTime * sensX;
        _xRotation -= lookInput.y * Time.deltaTime * sensY;
        
        // Clamp the X rotation to prevent over-rotation
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        // Rotate the camera
        orientation.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        
        // Update the orientation to match the camera's forward direction
        // orientation.transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
    }

    private void OnDestroy()
    {
        // // Disable the input action when the script is destroyed
        // lookAction.action.Disable();  
    }

    private void Update()
    {
        // OldCameraRotate();

        // Adjust camera angle based on wall running state
        // if (pm.isWallRunning && wr.wallRight)
        // {
        //     transform.rotation = Quaternion.Euler(xRotation, yRotation, -25f);
        // }
        // else if (pm.isWallRunning && wr.wallLeft)
        // {
        //     transform.rotation = Quaternion.Euler(xRotation, yRotation, 25f);
        // }

    }
}