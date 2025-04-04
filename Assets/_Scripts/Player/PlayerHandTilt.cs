using System;
using UnityEngine;

public class PlayerHandTilt : MonoBehaviour
{
    [SerializeField] private TransformReference playerOrientation;
    [SerializeField] private Vector3Reference playerVelocity;
    [SerializeField] private Animator animator;
    
    [SerializeField, Min(0.0001f)] private float speedThresholdLR = 18;
    [SerializeField, Min(0.0001f)] private float lerpAmount = .25f;
    
    private float _currentSwayAngleLR;
    
    private void Update()
    {
        // Get the normalized right vector of the orientation
        var right = playerOrientation.Value.right.normalized;

        // Get the dot product of the player's velocity and the right vector
        var rightVelocity = Vector3.Dot(playerVelocity.Value, right);
        var isLeft = rightVelocity < 0;

        var targetSwayLR = Mathf.InverseLerp(0, speedThresholdLR, Mathf.Abs(rightVelocity));
        if (isLeft)
            targetSwayLR *= -1;

        // Lerp the sway angle
        _currentSwayAngleLR = Mathf.Lerp(
            _currentSwayAngleLR, targetSwayLR * 1,
            CustomFunctions.FrameAmount(lerpAmount)
        );
        
        // TODO: Set the variable of the animator using the _currentSwayAngleLR variable
        if (animator != null)
            animator.SetFloat("WeaponSway", _currentSwayAngleLR);

    }
}