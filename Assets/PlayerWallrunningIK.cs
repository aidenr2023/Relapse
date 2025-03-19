using UnityEngine;

public class PlayerWallRunningIK : MonoBehaviour {
    
     [SerializeField] private PlayerWallRunning wallRunning;
    [SerializeField] private Animator animator;
    
    [Header("IK Settings")]
    [SerializeField] private float ikWeightTransitionSpeed = 8f; // Increased transition speed
    [SerializeField] [Range(0, 1)] private float maxArmExtension = 0.85f; // Arm extension limit
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private float forwardOffset = 0.3f;
    [SerializeField] private float handPositionOffset = 0.2f;

    [Header("Elbow Settings")]
    [SerializeField] private Vector3 elbowRotationOffset = new Vector3(-30, 0, 0);
    [SerializeField] private float positionPrediction = 0.1f; // Predictive positioning

    private float _currentIkWeight;
    private Vector3 _targetHandPosition;
    private Quaternion _targetHandRotation;
    private bool _isReloading;
    private Vector3 _velocity;

    private void OnAnimatorIK(int layerIndex)
    {
        if (!wallRunning.IsWallRunning || wallRunning.IsWallRunningRight || _isReloading)
        {
            _currentIkWeight = Mathf.Lerp(_currentIkWeight, 0, ikWeightTransitionSpeed * Time.deltaTime);
            ApplyHandIK(AvatarIKGoal.LeftHand, _currentIkWeight);
            return;
        }

        if (wallRunning.IsWallRunningLeft)
        {
            // Calculate predicted position based on velocity
            Vector3 predictedPosition = wallRunning.ContactInfo.point + 
                                      _velocity * positionPrediction;

            Vector3 wallNormal = wallRunning.ContactInfo.normal;
            Vector3 verticalOffsetPoint = predictedPosition + Vector3.up * verticalOffset;
            
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
            Vector3 forwardOffsetDirection = wallForward * forwardOffset;

            _targetHandPosition = verticalOffsetPoint + 
                               (wallNormal * handPositionOffset) + 
                               forwardOffsetDirection;

            // Apply elbow rotation offset
            _targetHandRotation = Quaternion.LookRotation(-wallNormal) * 
                                Quaternion.Euler(elbowRotationOffset);

            // Limit arm extension
            float distanceToShoulder = Vector3.Distance(
                animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position,
                _targetHandPosition
            );
            
            _currentIkWeight = Mathf.Lerp(_currentIkWeight, 
                Mathf.Clamp01(maxArmExtension / distanceToShoulder), 
                ikWeightTransitionSpeed * Time.deltaTime);

            ApplyHandIK(AvatarIKGoal.LeftHand, _currentIkWeight);
        }
    }

    private void Update()
    {
        // Track player velocity for prediction
        _velocity = wallRunning.GetComponent<Rigidbody>().velocity;
    }

    private void ApplyHandIK(AvatarIKGoal hand, float weight)
    {
        animator.SetIKPositionWeight(hand, weight);
        animator.SetIKRotationWeight(hand, weight);

        if (weight > 0.1f)
        {
            animator.SetIKPosition(hand, _targetHandPosition);
            animator.SetIKRotation(hand, _targetHandRotation);
        }
    }
    
    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying || !wallRunning.IsWallRunningLeft) return;
    //
    //     Gizmos.color = Color.cyan;
    //     Gizmos.DrawSphere(_targetHandPosition, 0.1f);
    //     Gizmos.DrawLine(_targetHandPosition, _targetHandPosition + transform.forward * 0.5f);
    // }

    public void OnReloadStart() => _isReloading = true;
    public void OnReloadComplete() => _isReloading = false;
    
    public void OnJump()
    {
        if (!wallRunning.IsWallRunning) _currentIkWeight = 0;
    }
}

    