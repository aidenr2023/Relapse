using System;
using UnityEngine;

[Serializable]
public sealed class DynamicRotationModule : DynamicVCamModule
{
    #region Serializable Fields

    [SerializeField] private Vector3 defaultRotation = Vector3.zero;

    [Space, SerializeField] private Vector3 wallRunRotation = new(0, 0, 10);
    [SerializeField, Range(0, 1)] private float wallRunLerpAmount = 0.05f;

    #endregion

    #region Private Fields

    private TokenManager<Vector3> _rotationTokens;

    private TokenManager<Vector3>.ManagedToken _wallRunToken;

    private bool _wallRunStart;
    private bool _wallRunEnd = true;

    private CinemachineRecomposer _recomposer;

    private float _wallRunningDirection;

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
    }

    public override void Start()
    {
        // Get the recomposer component
        _recomposer = playerVCamController.VirtualCamera.gameObject.GetComponent<CinemachineRecomposer>();

        // Add the wall run events
        if (playerVCamController.ParentComponent.PlayerController is PlayerMovementV2 movementV2)
        {
            movementV2.WallRunning.OnWallSlideStart += OnWallRunStart;
            movementV2.WallRunning.OnWallRunEnd += OnWallRunEnd;
        }
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

    #endregion

    public override void Update()
    {
        // Update the wall run rotation
        UpdateWallRunToken();

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
            if (wallRunning.IsWallRunningLeft)
                _wallRunningDirection = -1;
            else if (wallRunning.IsWallRunningRight)
                _wallRunningDirection = 1;
        }

        // Set the target value
        if (_wallRunStart)
            targetValue = wallRunRotation * _wallRunningDirection;
        else if (_wallRunEnd)
            targetValue = Vector3.zero;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // Set the wall run token value
        _wallRunToken.Value = Vector3.Lerp(_wallRunToken.Value, targetValue, wallRunLerpAmount * frameAmount);
    }

    private Vector3 CurrentTokenValue()
    {
        var value = Vector3.zero;

        foreach (var token in _rotationTokens.Tokens)
            value += token.Value;

        return value;
    }
}