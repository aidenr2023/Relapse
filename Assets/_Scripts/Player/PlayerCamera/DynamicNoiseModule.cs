using System;
using Cinemachine;
using UnityEngine;

[Serializable]
public sealed class DynamicNoiseModule : DynamicVCamModule
{
    #region Serializable Fields

    [SerializeField] private NoiseTokenValue defaultNoise;

    [Header("Ground Shake"), SerializeField]
    private NoiseTokenValue groundShakeNoise;

    [SerializeField, Range(0, 1)] private float groundCheckLerpAmount = 0.2f;
    [SerializeField, Min(0)] private float groundShakeTime;
    [SerializeField, Min(0)] private float groundShakeVelocityThreshold = 10f;

    [Header("Relapse"), SerializeField] private NoiseTokenValue relapseNoise;

    #endregion

    #region Private Fields

    private CinemachineBasicMultiChannelPerlin _noise;

    private TokenManager<NoiseTokenValue> _noiseTokens;

    private TokenManager<NoiseTokenValue>.ManagedToken _recoilToken;
    private TokenManager<NoiseTokenValue>.ManagedToken _relapseToken;
    private TokenManager<NoiseTokenValue>.ManagedToken _groundCheckToken;

    private CountdownTimer _recoilShakeTimer;
    private CountdownTimer _groundShakeTimer;

    private float _recoilLerpAmount;

    #endregion

    #region Getters

    public TokenManager<NoiseTokenValue> NoiseTokens => _noiseTokens;

    #endregion

    protected override void CustomInitialize(PlayerVirtualCameraController controller)
    {
        // Create the token manager
        _noiseTokens = new(false, null, default);

        // Create the tokens
        _recoilToken = _noiseTokens.AddToken(default, -1, true);
        _relapseToken = _noiseTokens.AddToken(default, -1, true);
        _groundCheckToken = _noiseTokens.AddToken(default, -1, true);

        // Create the shake timers
        _recoilShakeTimer = new CountdownTimer(0);
        _groundShakeTimer = new CountdownTimer(groundShakeTime);
    }

    public override void Start()
    {
        // Get the noise component
        _noise = playerVCamController.VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        // Subscribe to the OnLand event in the player movement script
        if (playerVCamController.ParentComponent.PlayerController is PlayerMovementV2 movementV2)
            movementV2.OnLand += ShakeOnLand;
    }

    private void ShakeOnLand(Vector3 landVelocity)
    {
        var landYVelocity = Mathf.Abs(landVelocity.y);

        // Check if the velocity is less than the threshold
        if (landYVelocity < groundShakeVelocityThreshold)
            return;

        _groundShakeTimer.SetMaxTimeAndReset(groundShakeTime);
        _groundShakeTimer.Start();

        _groundCheckToken.Value = groundShakeNoise;
    }

    public override void Update()
    {
        // Update the recoil token
        UpdateRecoilToken();

        // Update the relapse token
        UpdateRelapseToken();

        // Update the ground check token
        UpdateGroundCheckToken();

        // Update the noise tokens
        _noiseTokens.Update(Time.deltaTime);

        // Get the new noise information
        var newNoise = defaultNoise + CurrentTokenValue();

        // Set the new noise information
        _noise.m_AmplitudeGain = newNoise.AmplitudeGain;
        _noise.m_FrequencyGain = newNoise.FrequencyGain;
        _noise.m_PivotOffset = newNoise.PivotOffset;

        // Set the max times of the timers
        _groundShakeTimer.SetMaxTime(groundShakeTime);

        // Update the shake timers
        _recoilShakeTimer.Update(Time.deltaTime);
        _groundShakeTimer.Update(Time.deltaTime);
    }

    private void UpdateRecoilToken()
    {
        if (_recoilShakeTimer.Percentage < 1)
            return;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        _recoilToken.Value = NoiseTokenValue.Lerp(_recoilToken.Value, default, _recoilLerpAmount * frameAmount);
    }

    private void UpdateRelapseToken()
    {
        var desiredValue = default(NoiseTokenValue);

        if (playerVCamController.ParentComponent.PlayerInfo.IsRelapsing)
            desiredValue = relapseNoise;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        _relapseToken.Value = NoiseTokenValue.Lerp(_relapseToken.Value, desiredValue, .4f * frameAmount);
    }

    private void UpdateGroundCheckToken()
    {
        if (_groundShakeTimer.Percentage < 1)
            return;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        _groundCheckToken.Value =
            NoiseTokenValue.Lerp(_groundCheckToken.Value, default, groundCheckLerpAmount * frameAmount);
    }

    public void SetRecoilShake(NoiseTokenValue noiseToken, float lerpAmount, float time)
    {
        _recoilToken.Value = noiseToken;
        _recoilShakeTimer.SetMaxTimeAndReset(time);
        _recoilShakeTimer.Start();

        _recoilLerpAmount = lerpAmount;
    }

    private NoiseTokenValue CurrentTokenValue()
    {
        var tokenValue = default(NoiseTokenValue);

        foreach (var token in _noiseTokens.Tokens)
            tokenValue += token.Value;

        return tokenValue;
    }

    [Serializable]
    public struct NoiseTokenValue
    {
        [SerializeField] private Vector3 pivotOffset;
        [SerializeField] private float amplitudeGain;
        [SerializeField] private float frequencyGain;

        public Vector3 PivotOffset
        {
            get => pivotOffset;
            private set => pivotOffset = value;
        }

        public float AmplitudeGain
        {
            get => amplitudeGain;
            private set => amplitudeGain = value;
        }

        public float FrequencyGain
        {
            get => frequencyGain;
            private set => frequencyGain = value;
        }

        public NoiseTokenValue(Vector3 pivotOffset, float amplitudeGain, float frequencyGain)
        {
            this.pivotOffset = pivotOffset;
            this.amplitudeGain = amplitudeGain;
            this.frequencyGain = frequencyGain;
        }

        public static NoiseTokenValue Lerp(NoiseTokenValue a, NoiseTokenValue b, float t)
        {
            return new NoiseTokenValue(
                Vector3.Lerp(a.PivotOffset, b.PivotOffset, t),
                Mathf.Lerp(a.AmplitudeGain, b.AmplitudeGain, t),
                Mathf.Lerp(a.FrequencyGain, b.FrequencyGain, t)
            );
        }

        public static NoiseTokenValue operator +(NoiseTokenValue a, NoiseTokenValue b)
        {
            return new NoiseTokenValue(
                a.PivotOffset + b.PivotOffset,
                a.AmplitudeGain + b.AmplitudeGain,
                a.FrequencyGain + b.FrequencyGain
            );
        }

        public static NoiseTokenValue operator -(NoiseTokenValue a, NoiseTokenValue b)
        {
            return new NoiseTokenValue(
                a.PivotOffset - b.PivotOffset,
                a.AmplitudeGain - b.AmplitudeGain,
                a.FrequencyGain - b.FrequencyGain
            );
        }
    }
}