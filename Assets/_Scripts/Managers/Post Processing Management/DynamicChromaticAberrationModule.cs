using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicChromaticAberrationModule : DynamicPostProcessingModule
{
    #region Serialized Fields

    [SerializeField, Range(0, 1)] private float relapseLerpAmount = .8f;
    [SerializeField] private float relapseRange = .25f;

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _tokens = new(false, null, 0);

    private TokenManager<float>.ManagedToken _relapseToken;

    private TokenManager<float>.ManagedToken _powerToken;
    private float _maxPowerTokenValue;
    private CountdownTimer _powerTokenTimer;

    #endregion

    #region Getters

    public TokenManager<float> Tokens => _tokens;

    #endregion

    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
    }

    public override void Start()
    {
        // Add the relapse token
        _relapseToken = _tokens.AddToken(0, -1, true);

        // Add the power token
        _powerToken = _tokens.AddToken(0, -1, true);
        
        // Initialize the power token timer
        _powerTokenTimer = new CountdownTimer(0);
        _powerTokenTimer.OnTimerEnd += () => _powerTokenTimer.Stop();
    }

    public override void Update()
    {
        // Update the relapse token
        UpdateRelapseToken();

        // Update the power token
        UpdatePowerToken();

        // Get the actual screen chromatic aberration component
        dynamicVolume.GetActualComponent(out ChromaticAberration actualChromaticAberration);

        // Return if the screen chromatic aberration is null
        if (actualChromaticAberration == null)
            return;

        // Get the chromatic aberration settings
        dynamicVolume.GetSettingsComponent(out ChromaticAberration chromaticAberrationSettings);

        // Set the chromatic aberration intensity on the screen volume
        actualChromaticAberration.intensity.value = chromaticAberrationSettings.intensity.value + CurrentTokenValue();
    }

    private void UpdateRelapseToken()
    {
        // Get the player
        var player = Player.Instance;

        // Return if the player is null
        // Return if the player is dead
        // Return if the player is not relapsing
        if (player == null || player.PlayerInfo.CurrentHealth <= 0 || !player.PlayerInfo.IsRelapsing)
        {
            _relapseToken.Value = Mathf.Lerp(_relapseToken.Value, 0, relapseLerpAmount);

            if (Mathf.Abs(_relapseToken.Value) < .0001f)
                _relapseToken.Value = 0;

            return;
        }

        const int multiplier = 1000000;

        // Inclusively randomly generate a float from 0 to 1
        var relapseValue = UnityEngine.Random.Range(0, multiplier) / (float)multiplier * relapseRange;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // Lerp the relapse token value to the relapse value
        _relapseToken.Value = Mathf.Lerp(_relapseToken.Value, relapseValue, relapseLerpAmount * frameAmount);
    }

    private void UpdatePowerToken()
    {
        // Update the power token timer
        _powerTokenTimer.Update(Time.deltaTime);
        
        // Calculate the powerTokenValue
        var powerTokenValue = _maxPowerTokenValue * (1 - _powerTokenTimer.Percentage);
        
        // Add some randomness to the power token value
        powerTokenValue += UnityEngine.Random.Range(-.1f, .1f);
        
        // If the power token timer is not running, set the power token value to 0
        if (_powerTokenTimer.IsComplete || !_powerTokenTimer.IsActive)
            powerTokenValue = 0;
        
        // Set the power token value
        _powerToken.Value = powerTokenValue;
    }

    private float CurrentTokenValue()
    {
        var value = 0f;

        foreach (var token in _tokens.Tokens)
            value += token.Value;

        return value;
    }

    public void AddPowerToken(float value, float time)
    {
        _maxPowerTokenValue = Mathf.Max(_maxPowerTokenValue, value);

        // Reset the power token timer
        _powerTokenTimer.SetMaxTimeAndReset(time);
        _powerTokenTimer.Start();
        
        Debug.Log($"Added power token");
    }
}