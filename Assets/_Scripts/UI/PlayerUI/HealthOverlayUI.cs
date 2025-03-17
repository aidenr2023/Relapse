using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HealthOverlayUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private FloatReference playerCurrentHealth;

    [Space, SerializeField] private float minFlashTime = .25f;
    [SerializeField] private float maxFlashTime = 1;
    [SerializeField, Min(0)] private float healthForMaxFlashing = 50;
    [SerializeField] private float healthForMinFlashing = 10;
    [SerializeField] private float maxOpacity = .75f;

    #endregion

    private CountdownTimer _flashTimer;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Get the CanvasGroup component
        _canvasGroup = GetComponent<CanvasGroup>();

        // Set the alpha to 0
        _canvasGroup.alpha = 0;

        // Start the countdown timer
        _flashTimer = new CountdownTimer(maxFlashTime);

        // Set up the timer's events
        // Restart the timer
        _flashTimer.OnTimerEnd += () => _flashTimer.Reset();
    }

    private void OnEnable()
    {
        // Start the timer
        _flashTimer.Start();
    }

    private void OnDisable()
    {
        // Stop the timer
        _flashTimer.Stop();
    }

    private void Update()
    {
        _flashTimer.SetMaxTime(DetermineCurrentFlashTime());

        // Update the flash timer
        _flashTimer.Update(Time.deltaTime);

        // Set the image opacity
        SetImageOpacity();
    }

    private float DetermineCurrentFlashTime()
    {
        if (playerCurrentHealth >= healthForMaxFlashing)
            return maxFlashTime;

        var diff = healthForMaxFlashing - healthForMinFlashing;

        var healthPercentage = (playerCurrentHealth - healthForMinFlashing) / diff;

        return Mathf.Lerp(minFlashTime, maxFlashTime, healthPercentage);
    }

    private void SetImageOpacity()
    {
        // If the player's health is above the max flashing health, set the opacity to max
        if (playerCurrentHealth >= healthForMaxFlashing)
        {
            _canvasGroup.alpha = 0;
            return;
        }

        // Use a sin function from 0 to 1 to determine the opacity
        var sinAmount = Mathf.Sin(_flashTimer.Percentage * Mathf.PI) * 0.5f + 0.5f;

        // Determine the opacity based on the player's health
        var diff = healthForMaxFlashing - healthForMinFlashing;
        var healthPercentage = Mathf.Clamp01(1 - ((playerCurrentHealth - healthForMinFlashing) / diff));

        var opacity = (sinAmount * maxOpacity * healthPercentage);

        // Set the opacity
        _canvasGroup.alpha = opacity;
    }
}