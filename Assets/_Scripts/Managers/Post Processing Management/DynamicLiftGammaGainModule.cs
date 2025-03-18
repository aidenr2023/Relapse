using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicLiftGammaGainModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    [SerializeField] private UserSettingsVariable userSettings;
    
    [SerializeField, Range(-1, 1), Readonly]
    private float gammaSetting = 0;

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    private TokenManager<float>.ManagedToken _gammaSettingToken;

    #endregion

    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
        // Create the gamma token
        _gammaSettingToken = _tokens.AddToken(gammaSetting, -1, true);
    }

    public override void Start()
    {
    }

    public override void Update()
    {
        // update the gamma setting token
        UpdateGammaSettingToken();

        // Get the actual screen lift gamma gain component
        dynamicVolume.GetActualComponent(out LiftGammaGain actualLiftGammaGain);

        // Return if the screen lift gamma gain is null
        if (actualLiftGammaGain == null)
            return;

        // Get the lift gamma gain settings
        dynamicVolume.GetSettingsComponent(out LiftGammaGain liftGammaGainSettings);

        // Set the gamma value on the screen volume
        actualLiftGammaGain.gamma.value =
            new Vector4(
                liftGammaGainSettings.gamma.value.x,
                liftGammaGainSettings.gamma.value.y,
                liftGammaGainSettings.gamma.value.z,
                liftGammaGainSettings.gamma.value.w + CurrentTokenValue()
            );
    }

    private void UpdateGammaSettingToken()
    {
        // Pull the gamma setting from the user settings
        gammaSetting = userSettings.value.Gamma;

        // Apply the gamma setting to the token
        _gammaSettingToken.Value = gammaSetting;
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
        var castedModule = (DynamicLiftGammaGainModule) otherModule;
        
        // Transfer the tokens from the other module
        // Clear out the other token manager
        castedModule._tokens.Clear();
        
        // Add each of the current tokens to the other token manager
        foreach (var token in _tokens.Tokens)
            castedModule._tokens.ForceAddToken(token);
        
        // Individual tokens
        castedModule._gammaSettingToken = _gammaSettingToken;
    }
}