using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicVignetteModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float speedVignetteMaxValue = 0.5f;
    [SerializeField, Min(0)] private float speedVignetteMaxSpeed = 20;
    [SerializeField, Range(0, 1)] private float speedVignetteLerpAmount = 0.5f;

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    private TokenManager<float>.ManagedToken _speedVignetteToken;

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
    }

    public override void Update()
    {
        // Update the speed vignette
        UpdateSpeedVignette();

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

        const float defaultFrameTime = 1 / 60f;
        var frameTime = Time.deltaTime / defaultFrameTime;

        // Lerp the speed vignette token to the target value
        _speedVignetteToken.Value = Mathf.Lerp(_speedVignetteToken.Value, targetValue, speedVignetteLerpAmount * frameTime);
    }

    private float CurrentTokenValue()
    {
        var value = 0f;

        foreach (var token in _tokens.Tokens)
            value += token.Value;

        return value;
    }
}