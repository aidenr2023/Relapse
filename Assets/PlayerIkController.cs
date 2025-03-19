using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerIkController : MonoBehaviour
{
   [Header("References")]
    [SerializeField] private PlayerWallRunning wallRunningScript;
    [SerializeField] private Rig wallRunRig;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform shoulderBone;
    
    [Header("Rotation Constraint")]
    [SerializeField] private MultiRotationConstraint handRotationConstraint;
    [SerializeField] private Transform handRotationSource; // Assign the dummy GameObject
    
    [Header("Settings")]
    [SerializeField] private float handForwardOffset = 0.3f;
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private float ikTransitionSpeed = 10f;

    private void OnEnable()
    {
        // Subscribe to wall-running events
        wallRunningScript.OnWallRunStart += HandleWallRunStart;
        wallRunningScript.OnWallRunEnd += HandleWallRunEnd;
        wallRunningScript.OnWallChanged += HandleWallChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        wallRunningScript.OnWallRunStart -= HandleWallRunStart;
        wallRunningScript.OnWallRunEnd -= HandleWallRunEnd;
        wallRunningScript.OnWallChanged -= HandleWallChanged;
    }

    // --- Event Handlers ---
    private void HandleWallRunStart(PlayerWallRunning obj)
    {
        if (!obj.IsWallRunningLeft) return;

        // Activate IK rig and update rotation source
        wallRunRig.weight = 1f;
        UpdateHandPosition(obj.ContactInfo);
        UpdateRotationSource(obj.ContactInfo.normal);
    }

    private void HandleWallChanged(PlayerWallRunning obj)
    {
        if (!obj.IsWallRunningLeft) return;
        UpdateRotationSource(obj.ContactInfo.normal);
    }

    private void HandleWallRunEnd(PlayerWallRunning obj)
    {
        StartCoroutine(BlendOutIK());
    }
    
    //--- Update Loop ---
    private void LateUpdate()
    {
        if (!wallRunningScript.IsWallRunningLeft) return;
    
        // Get fresh wall contact info every frame
        RaycastHit contactInfo = wallRunningScript.ContactInfo;
    
        // Only update if we have valid wall contact
        if (contactInfo.collider == null) return;

        UpdateHandPosition(contactInfo);
    
        // Add movement-based offset
        ApplyMovementOffset(contactInfo);
    }
    /// <summary>
    /// Fields for ApplyMovementOffset
    /// </summary>
    [SerializeField] private float positionFollowSpeed = 4f;
    [SerializeField] private float maxPositionOffset = 0.5f;

    private Vector3 _currentWallForward;
    private float _positionAlongWall;

    private void ApplyMovementOffset(RaycastHit contactInfo)
    {
        // Calculate wall-aligned directions
        Vector3 wallRight = Vector3.Cross(contactInfo.normal, Vector3.forward).normalized;
        _currentWallForward = Vector3.Cross(contactInfo.normal, wallRight);

        // Get player's velocity along wall
        Vector3 lateralVelocity = Vector3.ProjectOnPlane(
            wallRunningScript.ParentComponent.Rigidbody.velocity, 
            contactInfo.normal
        );
    
        // Calculate position offset based on movement
        _positionAlongWall += Vector3.Dot(lateralVelocity, _currentWallForward) * Time.deltaTime;
        _positionAlongWall = Mathf.Clamp(_positionAlongWall, -maxPositionOffset, maxPositionOffset);
    }

    // --- Core Logic ---
    private void UpdateHandPosition(RaycastHit contactInfo)
    {
        if (shoulderBone == null) return;

        // Base position with movement offset
        Vector3 wallAnchorPoint = contactInfo.point + 
                                  _currentWallForward * _positionAlongWall;

        Vector3 handBasePos = new Vector3(
            wallAnchorPoint.x,
            shoulderBone.position.y + verticalOffset,
            wallAnchorPoint.z
        );

        // Add forward offset relative to wall
        Vector3 handPosition = handBasePos + 
                               _currentWallForward * handForwardOffset +
                               contactInfo.normal * 0.05f;

        // Smoothly update position
        leftHandTarget.position = Vector3.Lerp(
            leftHandTarget.position,
            handPosition,
            Time.deltaTime * ikTransitionSpeed
        );

        // Maintain rotation locked to wall surface
        leftHandTarget.rotation = Quaternion.Lerp(
            leftHandTarget.rotation,
            Quaternion.LookRotation(-contactInfo.normal),
            Time.deltaTime * ikTransitionSpeed
        );
    }
    private void UpdateRotationSource(Vector3 wallNormal)
    {
        // Rotate the dummy to align with the wall's normal
        handRotationSource.rotation = Quaternion.LookRotation(wallNormal);
    }

    

    private IEnumerator BlendOutIK()
    {
        float startWeight = wallRunRig.weight;
        float duration = 0.2f;
        
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            wallRunRig.weight = Mathf.Lerp(startWeight, 0, t/duration);
            yield return null;
        }
        wallRunRig.weight = 0;
    }
    
}