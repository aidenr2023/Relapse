using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToggleText : MonoBehaviour
{
    public Text toggleText; // Reference to the Text component

    private UiControls inputActions;

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new UiControls();
    }

    private void OnEnable()
    {
        // Enable the input actions
        inputActions.UiToggles.Enable();

        // Bind the P and O key actions
        inputActions.UiToggles.pToggle.performed += OnToggleP;
        inputActions.UiToggles.oToggle.performed += OnToggleO;
    }

    private void OnDisable()
    {
        // Unbind the actions to avoid memory leaks
        inputActions.UiToggles.pToggle.performed -= OnToggleP;
        inputActions.UiToggles.oToggle.performed -= OnToggleO;

        // Disable the input actions
        inputActions.UiToggles.Disable();
    }

    private void OnToggleP(InputAction.CallbackContext context)
    {
        // Show the text when P is pressed
        if (toggleText != null)
        {
            toggleText.gameObject.SetActive(true);
        }
    }

    private void OnToggleO(InputAction.CallbackContext context)
    {
        // Hide the text when O is pressed
        if (toggleText != null)
        {
            toggleText.gameObject.SetActive(false);
        }
    }
}