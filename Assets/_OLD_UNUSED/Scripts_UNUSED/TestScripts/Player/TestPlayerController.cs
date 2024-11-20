using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TestPlayerController : MonoBehaviour, IPlayerController
{
    private CharacterController _characterController;

    private Vector2 _movementInput;

    [SerializeField] private GameObject cameraPivot;

    [Header("Configuration")] [SerializeField]
    private float moveSpeed;

    [SerializeField] [Range(0.05f, 1)] private float aimSensitivity = 1;

    [SerializeField] [Range(0, 90)] [Tooltip("How limited (in degrees) the player's up and down look rotation is")]
    private float verticalRotationLimit;

    [SerializeField] private bool invertVerticalLook;
    [SerializeField] private bool invertHorizontalLook;

    #region Getters

    public Vector2 MovementInput => _movementInput;
    public GameObject CameraPivot => cameraPivot;
    public bool IsGrounded => _characterController.isGrounded;
    public bool IsSprinting => false;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the inputs
        InitializeInputs();
    }

    private void InitializeComponents()
    {
        // Get the CharacterController component
        _characterController = GetComponent<CharacterController>();
    }

    private void InitializeInputs()
    {
        // Movement
        InputManager.Instance.PlayerControls.PlayerMovementBasic.Move.performed += OnMovePerformed;
        InputManager.Instance.PlayerControls.PlayerMovementBasic.Move.canceled += OnMoveCanceled;

        InputManager.Instance.PlayerControls.Player.LookMouse.performed += OnLookPerformed;
    }

    #endregion

    #region Input Functions

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        _movementInput = obj.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        _movementInput = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext obj)
    {
        const float sensitivityMod = 0.25f;

        // Get the look input
        var lookInput = obj.ReadValue<Vector2>();

        // Handle Look inversion
        var invertX = invertHorizontalLook ? -1 : 1;
        var invertY = invertVerticalLook ? -1 : 1;

        // Rotate the player object for horizontal look
        transform.Rotate(Vector3.up, lookInput.x * aimSensitivity * sensitivityMod * invertX);

        // Rotate the camera pivot for vertical look
        cameraPivot.transform.Rotate(Vector3.left, lookInput.y * aimSensitivity * sensitivityMod * invertY);

        // Clamp the rotation around the x-axis
        var xRotation = cameraPivot.transform.eulerAngles.x;

        // Make sure the xRotation is between 0 and 360
        xRotation %= 360;

        // If the xRotation is greater than 180, subtract 360 from it
        if (xRotation > 180)
            xRotation -= 360;

        // Clamp the xRotation
        xRotation = Mathf.Clamp(
            xRotation,
            -90 + verticalRotationLimit,
            90 - verticalRotationLimit
        );

        // Apply the clamped rotation
        cameraPivot.transform.localEulerAngles = new Vector3(
            xRotation,
            0,
            0
        );
    }

    #endregion

    #region Update Functions

    // Update is called once per frame
    void Update()
    {
        // Update the movement of the player
        UpdateMove();
    }

    private void UpdateMove()
    {
        // Calculate the movement direction
        var sideMovement = transform.right * _movementInput.x;
        var forwardMovement = transform.forward * _movementInput.y;
        var moveDirection = (sideMovement + forwardMovement).normalized;

        // Move the player
        _characterController.SimpleMove(moveDirection * moveSpeed);
    }

    #endregion
}