using System;
using UnityEngine;

public class PositionRotationDampen : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;

    [SerializeField] private bool useFixedUpdate = true;

    [SerializeField] private float positionLerpAmount = .95f;
    [SerializeField] private float rotationLerpAmount = .95f;

    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 targetRotation;

    private Vector3 _previousWorldPosition;
    private Vector3 _previousWorldForward;

    private void OnDisable()
    {
        transform.localRotation = Quaternion.Euler(targetRotation);
        transform.localPosition = targetPosition;
    }

    private void Update()
    {
        if (!useFixedUpdate)
            Dampen();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            Dampen();
    }

    private void Dampen()
    {
        var localForward = transform.parent.forward;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        var newForward = Vector3.Lerp(
            _previousWorldForward,
            localForward,
            rotationLerpAmount * frameAmount
        );

        // Convert the newForward to a local forward vector
        var localForwardVector = transform.parent.InverseTransformDirection(newForward);

        // Create a local rotation from the local forward vector
        var localRotation = Quaternion.LookRotation(localForwardVector);

        // Set the local rotation
        transform.localRotation = Quaternion.Euler(
            localRotation.eulerAngles.x,
            localRotation.eulerAngles.y,
            0
        );

        _previousWorldForward = transform.forward;
    }
}