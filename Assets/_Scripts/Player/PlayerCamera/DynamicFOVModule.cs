using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class DynamicFOVModule : DynamicVCamModule
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float normalFOV = 60f;
    [SerializeField, Min(0)] private float maxFov = 150;

    [Space, SerializeField, Min(0)] private float sprintFOVMultiplier = 1.25f;
    [SerializeField, Range(0, 1)] private float sprintLerpAmount = 0.1f;

    [Space, SerializeField, Min(0)] private float dashFOVMultiplier = 1.5f;
    [SerializeField, Range(0, 1)] private float dashRunLerpAmount = 0.1f;

    [Space, SerializeField, Min(0)] private float aimInFovMultiplier = 0.75f;
    [Space, SerializeField, Min(0)] private float aimOutFovMultiplier = 1.25f;
    [SerializeField, Min(0)] private float aimLerpAmount = 0.2f;

    #endregion

    #region Private Fields

    private TokenManager<float> _fovTokens;

    // Tokens for the sprint and dash transitions
    private TokenManager<float>.ManagedToken _sprintToken;
    private TokenManager<float>.ManagedToken _dashToken;
    private TokenManager<float>.ManagedToken _aimToken;

    private bool _sprintStart;
    private bool _sprintEnd = true;
    private bool _dashStart;
    private bool _dashEnd = true;

    private bool _isAiming;

    #endregion

    #region Getters

    public TokenManager<float> FOVTokens => _fovTokens;

    #endregion

    protected override void CustomInitialize(PlayerVirtualCameraController controller)
    {
        // Initialize the token manager
        _fovTokens = new(false, null, 1f);

        // Create the tokens
        _sprintToken = _fovTokens.AddToken(1f, -1, true);
        _dashToken = _fovTokens.AddToken(1f, -1, true);
        _aimToken = _fovTokens.AddToken(1f, -1, true);
    }

    public override void Start()
    {
        if (playerVCamController.ParentComponent.PlayerController is PlayerMovementV2 movementV2)
        {
            movementV2.OnSprintStart += OnSprintStart;
            movementV2.OnSprintEnd += OnSprintEnd;

            movementV2.Dash.OnDashStart += OnDashStart;
            movementV2.Dash.OnDashEnd += OnDashEnd;
        }
    }

    #region Event Functions

    private void OnSprintStart()
    {
        // Set the flags
        _sprintStart = true;
        _sprintEnd = false;
    }


    private void OnSprintEnd()
    {
        // Set the flags
        _sprintStart = false;
        _sprintEnd = true;
    }


    private void OnDashStart(IDashScript obj)
    {
        // Set the flags
        _dashStart = true;
        _dashEnd = false;
    }

    private void OnDashEnd(IDashScript obj)
    {
        // Set the flags
        _dashStart = false;
        _dashEnd = true;
    }

    #endregion

    public override void Update()
    {
        // Update the tokens
        UpdateSprintToken();
        UpdateDashToken();
        UpdateAimToken();

        // Update the token manager
        _fovTokens.Update(Time.deltaTime);

        // Multiply the normal FOV by the current token value
        var newFOV = normalFOV * CurrentTokenValue();

        // Clamp the FOV to the max FOV
        newFOV = Mathf.Min(newFOV, maxFov);

        // Set the FOV of the virtual camera
        if (playerVCamController.VirtualCamera != null)
            playerVCamController.VirtualCamera.m_Lens.FieldOfView = newFOV;
    }

    private void UpdateSprintToken()
    {
        var moveMagnitude = playerVCamController.ParentComponent.PlayerController.MovementInput.magnitude;

        var targetValue = 1f;

        if ((_sprintStart && moveMagnitude >= .75f) ||
            (Player.Instance.PlayerController is PlayerMovementV2 movementV2 && movementV2.PlayerSlide.IsSliding)
           )
            targetValue = sprintFOVMultiplier;

        _sprintToken.Value = Mathf.Lerp(_sprintToken.Value, targetValue, CustomFunctions.FrameAmount(sprintLerpAmount));
    }

    private void UpdateDashToken()
    {
        var targetValue = 1f;

        if (_dashStart)
            targetValue = dashFOVMultiplier;

        _dashToken.Value = Mathf.Lerp(_dashToken.Value, targetValue, CustomFunctions.FrameAmount(dashRunLerpAmount));
    }

    private void UpdateAimToken()
    {
        var currentFovMultiplier = 1f;

        var powerManager = Player.Instance?.PlayerPowerManager;

        // Get the power manager from the player
        if (powerManager != null)
        {
            if (powerManager.CurrentPower != null)
            {
                // Set the aiming flag
                if (powerManager.CurrentPowerToken != null)
                    SetAiming(powerManager.CurrentPowerToken.ChargePercentage > 0);

                switch (powerManager.CurrentPower.FovZoomBehavior)
                {
                    default:
                    case PowerScriptableObject.PowerFovZoomBehavior.None:
                        currentFovMultiplier = 1f;
                        break;

                    case PowerScriptableObject.PowerFovZoomBehavior.In:
                        currentFovMultiplier = aimInFovMultiplier;
                        break;

                    case PowerScriptableObject.PowerFovZoomBehavior.Out:
                        currentFovMultiplier = aimOutFovMultiplier;
                        break;
                }
            }
        }

        var targetValue = _isAiming ? currentFovMultiplier : 1f;

        _aimToken.Value = Mathf.Lerp(_aimToken.Value, targetValue, CustomFunctions.FrameAmount(aimLerpAmount));
    }

    private float CurrentTokenValue()
    {
        var value = 1f;

        foreach (var token in _fovTokens.Tokens)
            value *= token.Value;

        return value;
    }

    public void SetAiming(bool isAiming)
    {
        _isAiming = isAiming;
    }
}