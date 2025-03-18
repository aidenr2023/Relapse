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
        if (!obj.IsWallRunningLeft) return; // Only activate for left runs
        wallRunRig.weight = 1f;
        UpdateHandPosition(obj.ContactInfo);
    }

    private void HandleWallChanged(PlayerWallRunning obj)
    {
        if (!obj.IsWallRunningLeft) return;
        UpdateHandPosition(obj.ContactInfo);
    }

    private void HandleWallRunEnd(PlayerWallRunning obj)
    {
        _positionAlongWall = 0;
        StartCoroutine(BlendOutIK());
    }
    
    //--- Update Loop ---
    private void LateUpdate()
    {
        if (!wallRunningScript.IsWallRunningLeft) return;
    
        // Get fresh wall data every frame
        RaycastHit contactInfo = wallRunningScript.ContactInfo;
        if (contactInfo.collider == null) return;

        // Calculate wall-aligned directions
        Vector3 wallRight = Vector3.Cross(contactInfo.normal, Vector3.up).normalized;
        Vector3 wallForward = Vector3.Cross(contactInfo.normal, wallRight);

        // Get player's movement direction along wall
        Vector3 playerVelocity = wallRunningScript.ParentComponent.Rigidbody.velocity;
        float velocityAlongWall = Vector3.Dot(playerVelocity, wallForward);

        // Update hand position with velocity offset
        Vector3 dynamicOffset = wallForward * (velocityAlongWall * Time.deltaTime * 0.5f);
    
        leftHandTarget.position += dynamicOffset;
        
        // Apply position offset
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
        // Convert contact point to wall's local space
        Vector3 wallLocalPos = contactInfo.collider.transform.InverseTransformPoint(contactInfo.point);
    
        // Maintain position in wall's space during movement
        Vector3 anchoredPosition = contactInfo.collider.transform.TransformPoint(wallLocalPos);
    
        // Apply vertical offset from shoulder
        anchoredPosition.y = shoulderBone.position.y + verticalOffset;

        // Add forward offset relative to WALL (not player)
        Vector3 wallForward = Vector3.Cross(contactInfo.normal, Vector3.up).normalized;
        Vector3 finalPosition = anchoredPosition + 
                                (wallForward * handForwardOffset) + 
                                (contactInfo.normal * 0.05f);

        leftHandTarget.position = Vector3.Lerp(
            leftHandTarget.position,
            finalPosition,
            Time.deltaTime * ikTransitionSpeed * 2f // Faster follow speed
        );
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