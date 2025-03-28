﻿using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicVignetteModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    [Header("Speed Vignette")] [SerializeField, Min(0)]
    private float speedVignetteMaxValue = 0.5f;

    [SerializeField, Min(0)] private float speedVignetteMaxSpeed = 20;
    [SerializeField, Range(0, 1)] private float speedVignetteLerpAmount = 0.5f;

    [Header("Slide Vignette")] [SerializeField, Min(0)]
    private float slideVignetteMaxValue = 0.5f;

    [SerializeField, Range(0, 1)] private float slideVignetteLerpAmount = 0.5f;

    [Header("Relapse Vignette")] [SerializeField, Min(0)]
    private float relapseVignetteMaxValue = 0.5f;

    [SerializeField, Range(0, 1)] private float relapseVignetteLerpAmount = 0.5f;

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    private TokenManager<float>.ManagedToken _speedVignetteToken;
    private TokenManager<float>.ManagedToken _slideVignetteToken;
    private TokenManager<float>.ManagedToken _relapseVignetteToken;

    #endregion

    #region Getters

    public TokenManager<float> Tokens => _tokens;

    #endregion

    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
    }

    public override void Start()
    {
        // Create the speed vignette token
        _speedVignetteToken = _tokens.AddToken(0, -1, true);
        _slideVignetteToken = _tokens.AddToken(0, -1, true);
        _relapseVignetteToken = _tokens.AddToken(0, -1, true);
    }

    public override void Update()
    {
        // Update the speed vignette
        UpdateSpeedVignette();

        // Update the slide vignette
        UpdateSlideVignette();

        // Update the relapse vignette
        UpdateRelapseVignette();

        // Get the actual screen vignette component
        dynamicVolume.GetActualComponent(out Vignette actualVignette);

        // Return if the screen vignette is null
        if (actualVignette == null)
            return;

        // Get the vignette settings
        dynamicVolume.GetSettingsComponent(out Vignette vignetteSettings);

        // Set the vignette intensity on the screen volume
        actualVignette.intensity.value = vignetteSettings.intensity.value + CurrentTokenValue();
    }

    private void UpdateRelapseVignette()
    {
        var player = Player.Instance;

        var targetValue = 0f;

        if (player != null && player.PlayerInfo.IsRelapsing)
            targetValue = relapseVignetteMaxValue;

        // lerp the relapse vignette token to the target value
        _relapseVignetteToken.Value = Mathf.Lerp(_relapseVignetteToken.Value, targetValue,
            CustomFunctions.FrameAmount(relapseVignetteLerpAmount));
    }

    private void UpdateSpeedVignette()
    {
        // Get the instance of the player
        var player = Player.Instance;

        var targetValue = 0f;

        if (player != null)
            targetValue = Mathf.Lerp(0,
                speedVignetteMaxValue,
                player.Rigidbody.velocity.magnitude / speedVignetteMaxSpeed
            );

        // Lerp the speed vignette token to the target value
        _speedVignetteToken.Value = Mathf.Lerp(_speedVignetteToken.Value, targetValue,
            CustomFunctions.FrameAmount(speedVignetteLerpAmount));
    }

    private void UpdateSlideVignette()
    {
        // Get the instance of the player
        var player = Player.Instance;

        var targetValue = 0f;

        var movementV2 = player?.PlayerController as PlayerMovementV2;

        if (player == null || movementV2 == null)
        {
            _slideVignetteToken.Value = 0;
            return;
        }

        // If the player is sliding, set the target value to the slide vignette max value
        if (movementV2.PlayerSlide.IsSliding)
            targetValue = slideVignetteMaxValue;

        // Lerp the speed vignette token to the target value
        _slideVignetteToken.Value = Mathf.Lerp(_slideVignetteToken.Value, targetValue,
            CustomFunctions.FrameAmount(slideVignetteLerpAmount));
    }

    private float CurrentTokenValue()
    {
        var value = 0f;

        foreach (var token in _tokens.Tokens)
            value += token.Value;

        return value;
    }

    public override void TransferTokens(DynamicPostProcessingModule otherModule)
    {
        var castedModule = (DynamicVignetteModule) otherModule;
        
        // Transfer the tokens from the other module
        // Clear out the other token manager
        castedModule._tokens.Clear();
        
        // Add each of the current tokens to the other token manager
        foreach (var token in _tokens.Tokens)
            castedModule._tokens.ForceAddToken(token);
        
        // Individual tokens
        castedModule._speedVignetteToken = _speedVignetteToken;
        castedModule._slideVignetteToken = _slideVignetteToken;
        castedModule._relapseVignetteToken = _relapseVignetteToken;
    }
}