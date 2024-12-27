using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Min(0)] private float opacityLerpAmount = .1f;

    [SerializeField, Min(0)] private float stayOnScreenTime = 1f;

    [SerializeField, Range(0, 1)] private float minOpacity = 0;
    [SerializeField, Range(0, 1)] private float maxOpacity = 1;

    #endregion

    #region Private Fields

    private Player _player;

    private float _previousStamina;

    private float _desiredOpacity;

    private CountdownTimer _stayOnScreenTimer;

    #endregion

    private void Awake()
    {
        // Create the stay on screen timer
        _stayOnScreenTimer = new CountdownTimer(stayOnScreenTime);
        _stayOnScreenTimer.Start();

        // Set the desired opacity to 0
        _desiredOpacity = canvasGroup.alpha = minOpacity;
    }

    private void Start()
    {
        _player = Player.Instance;
    }

    private void Update()
    {
        // Update the stay on screen timer
        _stayOnScreenTimer.SetMaxTime(stayOnScreenTime);
        _stayOnScreenTimer?.Update(Time.deltaTime);

        // if there is no player, try to get the player
        if (_player == null)
            _player = Player.Instance;

        // if the player is still null, return
        if (_player == null)
            return;

        // Get the player movement v2
        var movementV2 = _player.PlayerController as PlayerMovementV2;

        // if the player movement v2 is null, return
        if (movementV2 == null)
            return;

        // Calculate the stamina percentage
        var staminaPercentage = movementV2.CurrentStamina / movementV2.MaxStamina;

        // Change the slider value
        slider.value = staminaPercentage;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // If the previous stamina is greater than the current stamina, set the desired opacity to 1
        // If the current stamina is the max stamina, but the previous stamina was not, set the desired opacity to 0
        var isStaminaDecreasing = _previousStamina > movementV2.CurrentStamina;
        var isStaminaFull = staminaPercentage >= 1 && _previousStamina < movementV2.MaxStamina;

        if (isStaminaDecreasing)
        {
            // Set the desired opacity to 1
            _desiredOpacity = maxOpacity;

            // Reset the stay on screen timer
            _stayOnScreenTimer?.Reset();
        }

        // Set the desired opacity to 0
        else if (_stayOnScreenTimer?.IsComplete ?? false)
            _desiredOpacity = minOpacity;

        // Set the opacity of the images
        var newAlpha = Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, opacityLerpAmount * frameAmount);
        canvasGroup.alpha = newAlpha;

        // Set the previous stamina to the current stamina
        _previousStamina = movementV2.CurrentStamina;
    }
}