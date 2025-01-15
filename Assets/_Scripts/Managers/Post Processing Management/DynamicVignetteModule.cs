using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicVignetteModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    #endregion

    #region Getters

    public TokenManager<float> Tokens => _tokens;

    #endregion

    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
    }

    public override void Start()
    {
    }

    public override void Update()
    {
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

    private float CurrentTokenValue()
    {
        var value = 0f;

        foreach (var token in _tokens.Tokens)
            value += token.Value;

        return value;
    }
}