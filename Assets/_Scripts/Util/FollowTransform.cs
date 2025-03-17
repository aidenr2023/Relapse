using System;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private TransformReference targetTransform;
    [SerializeField] private Vector3Reference followOffset;
    [SerializeField] private bool useFixedUpdate = false;

    private void Update()
    {
        if (!useFixedUpdate)
            Follow();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            Follow();
    }

    private void Follow()
    {
        // If the target transform is null, return
        if (targetTransform == null)
            return;
        
        var targetPosition = targetTransform.Value.position;
        
        // Set the world space position of the object to the target position + the follow offset
        var newPosition = targetPosition + followOffset.Value;
        transform.position = newPosition;
    }
}