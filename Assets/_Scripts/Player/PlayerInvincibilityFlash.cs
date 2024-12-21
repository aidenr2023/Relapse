using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInvincibilityFlash : ComponentScript<Player>
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float flashCycleDuration = 0.1f;

    #endregion

    #region Serialized Fields

    private readonly HashSet<Renderer> _renderers = new();

    private bool _isFlashing;

    private float _flashTimer;

    #endregion

    private void Start()
    {
        // Subscribe to the OnDamaged event
        ParentComponent.PlayerInfo.OnDamaged += GetRenderersOnDamaged;
        ParentComponent.PlayerInfo.OnDamaged += StartFlashingOnDamaged;
    }

    private void GetRenderersOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Call the stop flashing method if the player is no longer invincible
        StopFlashing();

        // Get all the renderers in the player
        var renderers = ParentComponent.GetComponentsInChildren<Renderer>();

        // Add all the renderers to the hash set
        foreach (var cRenderer in renderers)
        {
            // If the renderer is not enabled, skip it
            if (!cRenderer.enabled)
                continue;

            // Add the renderer to the hash set
            _renderers.Add(cRenderer);
        }
    }

    private void StartFlashingOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Set the is flashing flag to true
        _isFlashing = true;

        // Reset the flash timer
        _flashTimer = 0;
    }

    private void StopFlashing()
    {
        // Undo all the changes made to the renderers

        // Enable all the renderers
        foreach (var cRenderer in _renderers)
        {
            // Skip the renderer if it is null
            if (cRenderer == null)
                continue;

            cRenderer.enabled = true;
        }

        // Clear the renderers
        _renderers.Clear();

        // Reset the is flashing flag
        _isFlashing = false;
    }

    private void Update()
    {
        // Call the stop flashing method if the player is no longer invincible
        if (_isFlashing && !ParentComponent.PlayerInfo.IsInvincibleBecauseDamaged)
            StopFlashing();

        // Update the flash timer
        _flashTimer += Time.deltaTime;
    }

    private void LateUpdate()
    {
        // Update the flash
        UpdateFlash();
    }

    private void UpdateFlash()
    {
        // Return if the player is not flashing
        if (!_isFlashing)
            return;

        // Check if the flash is on
        var isFlashOn = ((int)(_flashTimer / flashCycleDuration)) % 2 == 1;

        // Disable all the renderers
        foreach (var cRenderer in _renderers)
        {
            // Skip the renderer if it is null
            if (cRenderer == null)
                continue;

            cRenderer.enabled = isFlashOn;
        }
    }
}