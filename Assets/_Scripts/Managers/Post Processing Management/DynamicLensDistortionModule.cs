using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicLensDistortionModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    [SerializeField] private float dashDistortion = -1f;
    [SerializeField, Range(0, 1)] private float dashDistortionLerpAmount = 0.25f;

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    private TokenManager<float>.ManagedToken _dashToken;

    #endregion

    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
    }

    public override void Start()
    {
        // Create the dash token
        _dashToken = _tokens.AddToken(0, -1, true);
    }

    public override void Update()
    {
        // Update the dash token
        UpdateDashToken();

        // Get the actual screen lens distortion component
        dynamicVolume.GetActualComponent(out LensDistortion actualLensDistortion);

        // Return if the screen lens distortion is null
        if (actualLensDistortion == null)
            return;

        // Get the lens distortion settings
        dynamicVolume.GetSettingsComponent(out LensDistortion lensDistortionSettings);

        // Set the lens distortion intensity on the screen volume
        actualLensDistortion.intensity.value = lensDistortionSettings.intensity.value + CurrentTokenValue();
    }

    private void UpdateDashToken()
    {
        var targetValue = 0f;

        var player = Player.Instance;

        if (player != null && player.PlayerDash != null)
            targetValue = player.PlayerDash.IsDashing ? dashDistortion : 0;

        _dashToken.Value = Mathf.Lerp(_dashToken.Value, targetValue,
            CustomFunctions.FrameAmount(dashDistortionLerpAmount));
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
        var castedModule = (DynamicLensDistortionModule) otherModule;
        
        // Transfer the tokens from the other module
        // Clear out the other token manager
        castedModule._tokens.Clear();
        
        // Add each of the current tokens to the other token manager
        foreach (var token in _tokens.Tokens)
            castedModule._tokens.ForceAddToken(token);
        
        // Individual tokens
        castedModule._dashToken = _dashToken;
    }
}