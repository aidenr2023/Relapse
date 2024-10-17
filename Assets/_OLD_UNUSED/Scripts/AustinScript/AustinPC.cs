using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class AustinPC : MonoBehaviour
{
    private DefaultPlayerActions _defaultPlayerActions;
    private InputAction _moveAction;
    private InputAction _lookAction;

    private void Awake()
    {
        _defaultPlayerActions = new DefaultPlayerActions();
    }

    private void OnEnable()
    {
        _moveAction = _defaultPlayerActions.Player.Move;
        _moveAction.Enable();

        _lookAction = _defaultPlayerActions.Player.Look;
        _lookAction.Enable();

        _defaultPlayerActions.Player.Jump.performed += OnJump;
        _defaultPlayerActions.Player.Jump.Enable();
    }
    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();

        _defaultPlayerActions.Player.Jump.performed -= OnJump;
        _defaultPlayerActions.Player.Jump.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 moveDir = _moveAction.ReadValue<Vector2>();
        Vector2 lookDir = _moveAction.ReadValue<Vector2>();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump has been pressed");
    }


    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Quit the game
        if (Input.GetButton("Escape"))
        {
            Application.Quit();
        }
        //Basic Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetButton("Sprint");

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }


        characterController.Move(moveDirection * Time.deltaTime);
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}