using System;
using TMPro;
using UnityEngine;

public class InteractText : MonoBehaviour
{
    private const float LERP_THRESHOLD = 0.0001f;

    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TMP_Text interactText;

    [SerializeField, Range(0, 1)] private float hoverOverOpacity = 1;

    [SerializeField, Range(0, 1)] private float opacityLerpAmount = 0.15f;
    [SerializeField, Range(0, 1)] private float positionLerpAmount = 0.15f;

    [SerializeField] private Vector3 offset;

    [SerializeField] private float floatingBobAmount = 0.1f;
    [SerializeField, Min(0)] private float floatingBobFrequency = 1;

    [Header("Controls")] [SerializeField] private GameObject pcControls;
    [SerializeField] private string keyboardSchemeName;

    [SerializeField] private GameObject gamepadControls;
    [SerializeField] private string gamepadSchemeName;

    #endregion

    #region Private Fields

    private PlayerInteraction _playerInteraction;
    private IInteractable _currentInteractable;
    private IInteractable _previousInteractable;

    private float _desiredOpacity;
    private Vector3 _previousPosition;

    private CountdownTimer _positionResetTimer;

    private bool _resetPosition;

    #endregion

    private void Awake()
    {
        // Set the initial desired opacity to 0
        _desiredOpacity = 0;

        // Set the canvas group's alpha to 0
        canvasGroup.alpha = 0;

        // Create the position reset timer
        _positionResetTimer = new CountdownTimer(.5f);
        _positionResetTimer.OnTimerEnd += () => _resetPosition = true;
    }

    private void Update()
    {
        // Update the position reset timer
        _positionResetTimer.SetActive(_currentInteractable == null);
        _positionResetTimer.Update(Time.deltaTime);

        // Update the information of the current interactable
        UpdateInformation();

        // Update the position of this game object
        UpdatePosition();

        // Update the current controls text
        UpdateCurrentControlsText();

        // Update the desired opacity
        _desiredOpacity = UpdateDesiredOpacity();

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Lerp the alpha of the canvas group's alpha to the desired opacity
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, opacityLerpAmount * frameAmount);

        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < LERP_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;

        // Set the text of the interact text to the current interactable's interact text
        UpdateText();
    }

    private void LateUpdate()
    {
        // Set the previous interactable to the current interactable
        _previousInteractable = _currentInteractable;
    }

    private void UpdateInformation()
    {
        // Get the instance of the player object
        var player = Player.Instance;

        // Return if the player is null
        if (player == null)
        {
            _playerInteraction = null;
            _currentInteractable = null;
            return;
        }

        // Get the player's interaction script
        _playerInteraction = player.PlayerInteraction;

        // Return if the player interaction is null
        if (_playerInteraction == null || _playerInteraction.SelectedInteractable == null)
        {
            _currentInteractable = null;
            return;
        }

        // Set the current interactable to the player interaction's current interactable
        _currentInteractable = _playerInteraction.SelectedInteractable;

        // Reset the timer
        _positionResetTimer.Reset();
    }

    private void UpdatePosition()
    {
        var calculatedOffset = offset;

        // If there is a current interactable, set the previous position to the current interactable's position
        if (_currentInteractable != null)
        {
            // _previousPosition = _currentInteractable.GameObject.transform.position;
            _previousPosition = _playerInteraction.InteractionHitInfo.point;

            // Calculate the offset
            var xOffset = _playerInteraction.Player.PlayerController.CameraPivot.transform.right.normalized * offset.x;
            var yOffset = _playerInteraction.Player.PlayerController.CameraPivot.transform.up.normalized * offset.y;
            var zOffset = _playerInteraction.Player.PlayerController.CameraPivot.transform.forward.normalized *
                          offset.z;

            calculatedOffset = xOffset + yOffset + zOffset;

            // If the previous interactable is null, set the previous position to the current interactable's position
            // Force the transform to the current interactable's position
            if (_resetPosition)
            {
                _previousPosition = _playerInteraction.InteractionHitInfo.point;
                transform.position = _previousPosition;
                _resetPosition = false;
            }

            const float scaleDistance = 2;
            var scaleAmount = _playerInteraction.InteractionHitInfo.distance / scaleDistance;

            // If the player is within a certain range, the offset needs to be scaled down
            if (_playerInteraction.InteractionHitInfo.distance < 2)
                calculatedOffset *= Mathf.Min(scaleAmount, 1);
        }

        var sinePosition = Mathf.Sin(Mathf.PI * floatingBobFrequency * Time.unscaledTime) * floatingBobAmount;

        // Calculate the new position
        var newPosition = _previousPosition + calculatedOffset + new Vector3(0, sinePosition, 0);

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Set the position of this game object to the current interactable's position
        transform.position = Vector3.Lerp(transform.position, newPosition, positionLerpAmount * frameAmount);
    }

    private float UpdateDesiredOpacity()
    {
        // Return the default opacity if there is no selected interactable
        if (_currentInteractable == null)
            return 0;

        // If the menu manager has any menus that disable controls, return 0
        if (MenuManager.Instance.IsControlsDisabledInMenus)
            return 0;

        // Since there is a selected interactable, set the desired opacity to the hover over opacity
        return hoverOverOpacity;
    }

    private void UpdateText()
    {
        // Return if there is no selected interactable
        if (_currentInteractable == null)
        {
            // interactText.text = "";
            return;
        }

        // Return if the player interaction is null
        if (_playerInteraction == null)
        {
            // interactText.text = "";
            return;
        }

        // Set the text of the interact text to the current interactable's interact text
        interactText.text = _currentInteractable.InteractText(_playerInteraction);
    }

    private void UpdateCurrentControlsText()
    {
        // Disable all the controls text
        pcControls?.SetActive(false);
        gamepadControls?.SetActive(false);

        // Set the current controls text based on the current control scheme
        if (InputManager.Instance.CurrentControlScheme == InputManager.ControlSchemeType.Gamepad)
            gamepadControls?.SetActive(true);
        else
            pcControls?.SetActive(true);
    }
}