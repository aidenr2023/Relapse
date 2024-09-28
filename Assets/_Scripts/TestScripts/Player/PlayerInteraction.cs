using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] [Min(0)] private float interactDistance = 5;

    [SerializeField] private TMP_Text interactText;

    /// <summary>
    /// A reference to the interactable that the player is currently looking at.
    /// </summary>
    private IInteractable _selectedInteractable;

    #region Getters

    public TestPlayer Player { get; private set; }

    public IInteractable SelectedInteractable => _selectedInteractable;

    #endregion

    private void Awake()
    {
        // Get the player component
        Player = GetComponent<TestPlayer>();
    }

    private void Start()
    {
        // Initialize the controls
        InitializeControls();
    }

    #region Controls

    private void InitializeControls()
    {
        // Subscribe to the interact event
        InputManager.Instance.PlayerControls.GamePlay.Interact.performed += OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        // If the player is not currently looking at an interactable, return
        if (_selectedInteractable == null)
            return;

        // Interact with the current interactable
        _selectedInteractable.Interact(this);
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the interactable if it is being looked at
        _selectedInteractable?.LookAtUpdate(this);

        // Update the interact text
        UpdateInteractText();
    }

    private void FixedUpdate()
    {
        // Detect the currently selected interactable
        UpdateSelectedInteractable();
    }

    private void UpdateSelectedInteractable()
    {
        // // Reset the currently looked at flag for the previous interactable
        // if (_selectedInteractable != null)
        //     _selectedInteractable.IsCurrentlySelected = false;

        // Get the camera pivot
        var cameraPivot = Player.PlayerController.CameraPivot.transform;

        // Is there a ray cast hit within the interact distance?
        var hit = Physics.Raycast(
            cameraPivot.position,
            cameraPivot.forward,
            out var hitInfo,
            interactDistance
        );

        // Perform a raycast to see if the player is looking at an interactable
        if (hit)
        {
            // If the player is looking at an interactable,
            // set the current selected interactable to the interactable that the player is looking at
            if (hitInfo.collider.TryGetComponent(out IInteractable interactable))
            {
                // If the interactable is interactable,
                // set the current selected interactable to the interactable
                if (interactable.IsInteractable)
                    _selectedInteractable = interactable;

                // Otherwise, set the current selected interactable to null
                else _selectedInteractable = null;
            }

            // If there is no interactable, set the current selected interactable to null
            else _selectedInteractable = null;
        }

        // If the player is not looking at an interactable, set the current selected interactable to null
        else _selectedInteractable = null;

        // // Set the currently looked at flag for the new interactable
        // if (_selectedInteractable != null)
        //     _selectedInteractable.IsCurrentlySelected = true;
    }

    private void UpdateInteractText()
    {
        // If the player is not looking at an interactable, hide the interact text
        if (_selectedInteractable == null)
        {
            interactText.gameObject.SetActive(false);
            return;
        }

        // Show the interact text
        interactText.gameObject.SetActive(true);

        // Get the object's interact text
        var interactTextString = _selectedInteractable.InteractText(this);

        // If the interact text is empty,
        // set the interact text to the default interact text
        if (interactTextString == string.Empty)
            interactText.text = "Press E to interact";

        // Set the interact text to the interactable's interact text
        else
            interactText.text = $"Press E to\n{interactTextString}";
    }

    #endregion

    private void OnDrawGizmos()
    {
        // Return if the selected interactable is null
        if (_selectedInteractable == null)
            return;

        // Draw a line from the camera pivot to the interactable
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            Player.PlayerController.CameraPivot.transform.position,
            _selectedInteractable.GameObject.transform.position
        );

        // Draw a sphere at the interactable's position
        Gizmos.DrawSphere(_selectedInteractable.GameObject.transform.position, 0.5f);
    }
}