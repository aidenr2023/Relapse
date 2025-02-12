using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthImage : MonoBehaviour
{
    [SerializeField] private Player player;

    // [SerializeField] private Image healthImage;

    [SerializeField] private float minFlashTime = 0.1f;
    [SerializeField] private float maxFlashTime = 0.5f;

    [Tooltip("If the player's health is below this number, start flashing the health image.")] [SerializeField] [Min(0)]
    private float healthForMaxFlashing = 75f;

    [SerializeField] [Min(0)] private float healthForMinFlashing = 20f;

    [SerializeField] [Range(0, 1)] private float maxOpacity;

    private CountdownTimer _flashTimer;

    private void Awake()
    {
        // Start the countdown timer
        _flashTimer = new CountdownTimer(maxFlashTime);

        // Set up the timer's events
        // Restart the timer
        _flashTimer.OnTimerEnd += () => _flashTimer.Reset();

        // Start the timer
        _flashTimer.Start();
    }

    private void Update()
    {
        _flashTimer.SetMaxTime(DetermineCurrentFlashTime());

        // Update the flash timer
        _flashTimer?.Update(Time.deltaTime);

        // Set the image opacity
        SetImageOpacity();

        // Decrease the health
        if (Input.GetKeyDown(KeyCode.K))
            player.PlayerInfo.ChangeHealth(-10, player.PlayerInfo, player.PlayerInfo, player.transform.position);
    }

    private float DetermineCurrentFlashTime()
    {
        if (player.PlayerInfo.CurrentHealth >= healthForMaxFlashing)
            return maxFlashTime;

        var diff = healthForMaxFlashing - healthForMinFlashing;

        var healthPercentage = (player.PlayerInfo.CurrentHealth - healthForMinFlashing) / diff;

        return Mathf.Lerp(minFlashTime, maxFlashTime, healthPercentage);
    }

    private void SetImageOpacity()
    {
        // If the player's health is above the max flashing health, set the opacity to max
        if (player.PlayerInfo.CurrentHealth >= healthForMaxFlashing)
        {
            HealthOverlayUI.Instance.CanvasGroup.alpha = 0;
            return;
        }

        // Use a sin function from 0 to 1 to determine the opacity
        var sinAmount = Mathf.Sin(_flashTimer.Percentage * Mathf.PI) * 0.5f + 0.5f;

        // Determine the opacity based on the player's health
        var diff = healthForMaxFlashing - healthForMinFlashing;
        var healthPercentage = Mathf.Clamp01(1 - ((player.PlayerInfo.CurrentHealth - healthForMinFlashing) / diff));

        var opacity = (sinAmount * maxOpacity * healthPercentage);

        // Set the opacity
        HealthOverlayUI.Instance.CanvasGroup.alpha = opacity;
    }
}