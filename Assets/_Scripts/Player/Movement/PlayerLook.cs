using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization; // Include the new Input System namespace

public class PlayerLook : MonoBehaviour
{
    // Mouse sensitivity
    [Header("Settings")] [SerializeField] [Min(1)]
    private float sensitivityMultiplier = 50;

    [SerializeField] [Range(0.01f, 1)] private float sensX = 1;
    [SerializeField] [Range(0.01f, 1)] private float sensY = 1;

    [Header("References")]
    // Reference to PlayerMovement script
    [SerializeField]
    private PlayerMovement playerMovement;

    // Reference to WallRunning script
    [SerializeField] private WallRunning wallRunning;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    // Current rotation around the X axis
    private float _xRotation;

    // Current rotation around the Y axis
    private float _yRotation;

    private void Start()
    {
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

        // Calculate the constant sensitivity multiplier that does not 
        // depend on x or y sensitivity / input
        var constantSense = sensitivityMultiplier * Time.deltaTime;

        // Adjust rotation based on mouse input
        _yRotation += lookInput.x * sensX * constantSense;
        _xRotation -= lookInput.y * sensY * constantSense;

        // Clamp the X rotation to prevent over-rotation
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Rotate the camera
        orientation.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }

    private void OnDestroy()
    {
        InputManager.Instance.PlayerControls.GamePlay.Look.performed -= OnLook;
    }

    private void Update()
    {
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