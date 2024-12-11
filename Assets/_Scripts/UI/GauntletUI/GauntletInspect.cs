using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GauntletInspectController : MonoBehaviour, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private Player player;

    #endregion

    #region Private Fields

    // Reference to the Animator component
    private Animator _animator;

    // Flag to track if the animation is currently paused
    private bool _isPaused;

    // Flag to ensure PauseAnimation is only called once per event
    private bool _hasPaused;

    private bool _isInspectHeld;

    #endregion

    public HashSet<InputData> InputActions { get; } = new();

    private void Awake()
    {
        // Get the animator component attached to this GameObject
        _animator = GetComponent<Animator>();

        // Error handling if Animator is not found
        if (_animator == null)
            Debug.LogError($"GauntletInspectController: Animator component not found on {gameObject.name}");

        // Initialize flags
        _isPaused = false;
        _hasPaused = false;

        // Assert that the player is not null
        Debug.Assert(player != null, "GauntletInspectController: Player is null.");

        // Initialize the input
        InitializeInput();
    }

    private void OnEnable()
    {
        // Register the input
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister the input
        InputManager.Instance.Unregister(this);
    }

    public void InitializeInput()
    {
        // Set up input actions
        InputActions.Add(
            new InputData(InputManager.Instance.pControls.Player.Inspect, InputType.Performed, OnInspectPerformed)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.pControls.Player.Inspect, InputType.Canceled, OnInspectCanceled)
        );
    }

    private void OnInspectPerformed(InputAction.CallbackContext context)
    {
        TriggerGauntletInspect();

        // Set the flag to true
        _isInspectHeld = true;
    }

    private void OnInspectCanceled(InputAction.CallbackContext context)
    {
        // Set the flag to false
        _isInspectHeld = false;

        if (_isPaused)
            ResumeAnimation();
    }

    /// <summary>
    /// Triggers the GauntletInspect animation.
    /// </summary>
    private void TriggerGauntletInspect()
    {
        if (_animator == null)
        {
            Debug.LogWarning("GauntletInspectController: Animator not assigned. Cannot trigger animation.");
            return;
        }

        _animator.SetTrigger("PlayTrigger");
        Debug.Log("GauntletInspectController: PlayTrigger activated.");
    }

    /// <summary>
    /// Called by the Animation Event to pause the animation.
    /// </summary>
    public void PauseAnimation()
    {
        if (_animator == null)
            return;

        // Ensure the Animator exists and the "I" key is being held
        if (_isInspectHeld)
        {
            if (!_hasPaused)
            {
                _animator.speed = 0f; // Pause the animation
                _isPaused = true;
                _hasPaused = true; // Prevent multiple pauses from multiple events
                Debug.Log("GauntletInspectController: Animation paused via PauseAnimation event.");
            }
            else
                Debug.Log("GauntletInspectController: Animation already paused.");
        }
        else
            Debug.Log("GauntletInspectController: PauseAnimation event triggered, but 'I' key is not held.");
    }

    /// <summary>
    /// Resumes the animation from where it was paused.
    /// </summary>
    private void ResumeAnimation()
    {
        if (_animator == null)
        {
            Debug.LogWarning("GauntletInspectController: Animator not assigned. Cannot resume animation.");
            return;
        }

        // Resume the animation
        _animator.speed = 1f;
        _isPaused = false;

        // Reset for potential future pauses
        _hasPaused = false;
        Debug.Log("GauntletInspectController: Animation resumed after releasing 'I' key.");
    }
}