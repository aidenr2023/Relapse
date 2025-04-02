using System;
using UnityEngine;

[RequireComponent(typeof(PlayerVirtualCameraController))]
public class PlayerVelocitySway : MonoBehaviour
{
    [SerializeField] private TransformReference playerOrientation;
    [SerializeField] private Vector3Reference playerVelocity;

    [SerializeField, Min(0)] private float maxSwayAngleLR;
    [SerializeField, Min(0.0001f)] private float swaySpeedThresholdLR;

    [SerializeField, Min(0)] private float maxSwayAngleFB;
    [SerializeField, Min(0.0001f)] private float swaySpeedThresholdFB;
    [SerializeField] private float lerpAmount = .25f;

    private PlayerVirtualCameraController _vCamController;
    private TokenManager<Vector3>.ManagedToken _swayToken;

    private float _currentSwayAngleLR;
    private float _currentSwayAngleFB;

    private void Awake()
    {
        _vCamController = GetComponent<PlayerVirtualCameraController>();
    }

    private void Start()
    {
        // Add the sway token to the dynamic rotation module
        _swayToken = _vCamController.DynamicRotationModule.RotationTokens.AddToken(Vector3.zero, -1, true);
    }

    private void OnDisable()
    {
        _swayToken.Value = Vector3.zero;
    }

    private void Update()
    {
        // Get the normalized right vector of the orientation
        var right = playerOrientation.Value.right.normalized;

        // Get the normalized forward vector of the orientation
        var forward = playerOrientation.Value.forward.normalized;

        // Get the dot product of the player's velocity and the right vector
        var rightVelocity = Vector3.Dot(playerVelocity.Value, right);
        var isLeft = rightVelocity < 0;

        // Get the dot product of the player's velocity and the forward vector
        var forwardVelocity = Vector3.Dot(playerVelocity.Value, forward);
        var isBackward = forwardVelocity < 0;

        var targetSwayLR = Mathf.InverseLerp(0, swaySpeedThresholdLR, Mathf.Abs(rightVelocity));
        if (isLeft)
            targetSwayLR *= -1;

        var targetSwayFB = Mathf.InverseLerp(0, swaySpeedThresholdFB, Mathf.Abs(forwardVelocity));
        if (isBackward)
            targetSwayFB *= -1;

        // Lerp the sway angle
        _currentSwayAngleLR = Mathf.Lerp(
            _currentSwayAngleLR, targetSwayLR * maxSwayAngleLR,
            CustomFunctions.FrameAmount(lerpAmount)
        );
        _currentSwayAngleFB = Mathf.Lerp(
            _currentSwayAngleFB, targetSwayFB * maxSwayAngleFB,
            CustomFunctions.FrameAmount(lerpAmount)
        );

        // Update the value of the sway token
        _swayToken.Value = new Vector3(_currentSwayAngleFB, 0, -_currentSwayAngleLR);
    }
}