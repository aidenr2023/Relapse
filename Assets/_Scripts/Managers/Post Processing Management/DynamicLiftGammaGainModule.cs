using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicLiftGammaGainModule : DynamicPostProcessingModule
{
    #region Serialized Fields

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
        gammaSetting = UserSettings.Instance.Gamma;

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
}