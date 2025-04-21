using System;
using UnityEngine;

public class CopyTransformRotation : MonoBehaviour
{
    [SerializeField] private TransformReference targetTransform;
    [SerializeField] private Vector3 rotationOffset;
    
    [Header("Copy"), SerializeField] private bool copyX = true;
    [SerializeField] private bool copyY = true;
    [SerializeField] private bool copyZ = true;
    
    [Header("Invert"), SerializeField] private bool invertX;
    [SerializeField] private bool invertY;
    [SerializeField] private bool invertZ;
    
    private void Update()
    {
        // Copy the rotation
        CopyRotation();
    }
    
    public void CopyRotation()
    {
        if (targetTransform == null || targetTransform.Value == null)
            return;

        var target = targetTransform.Value;
        
        var rotation = transform.rotation.eulerAngles;
        var targetRotation = target.rotation.eulerAngles;

        if (copyX)
            rotation.x = invertX ? -targetRotation.x : targetRotation.x;
        
        if (copyY)
            rotation.y = invertY ? -targetRotation.y : targetRotation.y;
        
        if (copyZ)
            rotation.z = invertZ ? -targetRotation.z : targetRotation.z;

        // Apply the rotation to this transform
        transform.rotation = Quaternion.Euler(rotation + rotationOffset);
    }
}