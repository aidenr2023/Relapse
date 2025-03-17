using System;
using UnityEngine;

[Serializable]
public sealed class DynamicRotationModule : DynamicVCamModule
{
    #region Serializable Fields

    [SerializeField] private Vector3 defaultRotation = Vector3.zero;

    [Header("Wall Running"), SerializeField]
    private Vector3 wallRunRotation = new(0, 0, 10);

    [SerializeField] private Vector3 wallClimbRotation = new(-10, 0, 0);
    [SerializeField] private Vector3 downwardWallClimbRotation = new(10, 0, 0);

    [SerializeField, Range(0, 1)] private float wallRunLerpAmount = 0.2f;

    [Header("Flinching"), SerializeField] private Vector3 flinchRotation = new(0, 0, 10);
    [SerializeField, Range(0, 1)] private float flinchLerpAmount = 0.2f;
    [SerializeField, Min(0)] private float flinchRecoveryThreshold = 1f;
    [SerializeField, Min(0)] private float maxFlinchDamageThreshold = 40;

    #endregion

    #region Private Fields

    private TokenManager<Vector3> _rotationTokens;

    private TokenManager<Vector3>.ManagedToken _wallRunToken;
    private TokenManager<Vector3>.ManagedToken _wallClimbToken;
    private TokenManager<Vector3>.ManagedToken _flinchToken;

    private bool _wallRunStart;
    private bool _wallRunEnd = true;

    private bool _wallClimbStart;
    private bool _wallClimbEnd = true;

    private CinemachineRecomposer _recomposer;

    private float _wallRunningDirection;

    private Vector3 _desiredFlinchValue;

    #endregion

    #region Getters

    public TokenManager<Vector3> RotationTokens => _rotationTokens;

    #endregion

    protected override void CustomInitialize(PlayerVirtualCameraController controller)
    {
        // Initialize the token manager
        _rotationTokens = new(false, null, Vector3.zero);

        // Create the tokens
        _wallRunToken = _rotationTokens.AddToken(Vector3.zero, -1, true);
        _wallClimbToken = _rotationTokens.AddToken(Vector3.zero, -1, true);
        _flinchToken = _rotationTokens.AddToken(Vector3.zero, -1, true);
    }

    protected override void CustomStart()
    {
        // Get the recomposer component
        _recomposer = playerVCamController.VirtualCamera.gameObject.GetComponent<CinemachineRecomposer>();

        // Add the wall run events
        if (playerVCamController.ParentComponent.PlayerController is PlayerMovementV2 movementV2)
        {
            movementV2.WallRunning.OnWallSlideStart += OnWallRunStart;
            movementV2.WallRunning.OnWallRunEnd += OnWallRunEnd;

            // Add the wall climb event
            movementV2.WallRunning.OnWallClimbStart += OnWallClimbStart;
            movementV2.WallRunning.OnWallClimbEnd += OnWallClimbEnd;
        }

        // Add the flinch event
        playerVCamController.ParentComponent.PlayerInfo.OnDamaged += FlinchOnDamaged;
    }

    private void FlinchOnDamaged(object sender, HealthChangedEventArgs e)
    {
        var xDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        var yDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        var zDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;

        var randomDirection = new Vector3(xDirection, yDirection, zDirection);

        var flinchAmount = Mathf.Clamp01(Mathf.Abs(e.Amount / maxFlinchDamageThreshold));

        // Set the desired flinch value
        _desiredFlinchValue = new Vector3(
            flinchRotation.x * randomDirection.x,
            flinchRotation.y * randomDirection.y,
            flinchRotation.z * randomDirection.z
        ) * flinchAmount;
    }

    #region Event Functions

    private void OnWallRunStart(PlayerWallRunning obj)
    {
        // Update the flags
        _wallRunStart = true;
        _wallRunEnd = false;
    }

    private void OnWallRunEnd(PlayerWallRunning obj)
    {
        // Update the flags
        _wallRunStart = false;
        _wallRunEnd = true;
    }

    private void OnWallClimbStart(PlayerWallRunning obj)
    {
        // Update the flags
        _wallClimbStart = true;
        _wallClimbEnd = false;
    }

    private void OnWallClimbEnd(PlayerWallRunning obj)
    {
        // Update the flags
        _wallClimbStart = false;
        _wallClimbEnd = true;
    }

    #endregion

    public override void Update()
    {
        // Update the wall run rotation
        UpdateWallRunToken();

        // Update the wall climb rotation
        UpdateWallClimbToken();

        // Update the flinch rotation
        UpdateFlinchToken();

        // Update the token manager
        _rotationTokens.Update(Time.deltaTime);

        // Get the new rotation
        var newRotation = defaultRotation + CurrentTokenValue();

        // Set the new rotation
        _recomposer.m_Tilt = newRotation.x;
        _recomposer.m_Pan = newRotation.y;
        _recomposer.m_Dutch = newRotation.z;
    }

    private void UpdateWallRunToken()
    {
        // Get the target value
        var targetValue = _wallRunToken.Value;

        // Determine the wall running direction
        var wallRunning = (playerVCamController.ParentComponent.PlayerController as PlayerMovementV2)?.WallRunning;

        if (wallRunning != null)
        {
            if (!wallRunning.IsWallRunning)
                _wallRunningDirection = 0;
            else if (wallRunning.IsWallRunningLeft)
                _wallRunningDirection = -1;
            else if (wallRunning.IsWallRunningRight)
                _wallRunningDirection = 1;
        }

        // Set the target value
        if (_wallRunStart)
            targetValue = wallRunRotation * _wallRunningDirection;
        else if (_wallRunEnd)
            targetValue = Vector3.zero;

        // Set the wall run token value
        _wallRunToken.Value = Vector3.Lerp(_wallRunToken.Value, targetValue, CustomFunctions.FrameAmount(wallRunLerpAmount));
    }

    private void UpdateWallClimbToken()
    {
        // Get the target value
        var targetValue = _wallClimbToken.Value;

        // Set the target value
        if (_wallClimbStart)
        {
            targetValue = wallClimbRotation;
            
            // If the player is falling, invert the rotation
            if (playerVCamController.ParentComponent.PlayerController is PlayerMovementV2 movementV2)
            {
                if (movementV2.Rigidbody.velocity.y < 0)
                    targetValue = downwardWallClimbRotation;
            }
        }
        else if (_wallClimbEnd)
            targetValue = Vector3.zero;

        // Set the wall run token value
        _wallClimbToken.Value = Vector3.Lerp(_wallClimbToken.Value, targetValue, CustomFunctions.FrameAmount(wallRunLerpAmount));
    }

    private void UpdateFlinchToken()
    {
        if (Vector3.Distance(_flinchToken.Value, _desiredFlinchValue) < flinchRecoveryThreshold)
            _desiredFlinchValue = Vector3.zero;

        _flinchToken.Value = Vector3.Lerp(_flinchToken.Value, _desiredFlinchValue, flinchLerpAmount);
    }

    private Vector3 CurrentTokenValue()
    {
        var value = Vector3.zero;

        foreach (var token in _rotationTokens.Tokens)
            value += token.Value;

        return value;
    }
}