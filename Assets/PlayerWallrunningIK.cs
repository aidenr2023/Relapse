using System;
using UnityEngine;

public class PlayerWallRunningIK : MonoBehaviour
{
    [SerializeField] private PlayerWallRunning wallRunning;
    [SerializeField] private Animator animator;
    
    [Header("Position Settings")]
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private float forwardOffset = 0.3f;
    [SerializeField] private float handPositionOffset = 0.2f;
    [SerializeField] private float positionPrediction = 0.1f;

    [Header("Rotation Settings")]
    [Tooltip("Adjust hand rotation around different axes")]
    [SerializeField] private Vector3 handRotationOffset = new Vector3(-20, 10, 5);
    [Tooltip("Control elbow bend amount")]
    [SerializeField] private Vector3 elbowRotationOffset = new Vector3(-30, 0, 0);
    [SerializeField] [Range(0, 1)] private float rotationSmoothing = 0.2f;

    [Header("IK Weights")]
    [SerializeField] private float ikWeightTransitionSpeed = 8f;
    [SerializeField] [Range(0, 1)] private float maxArmExtension = 0.85f;
    
   // [SerializeField] WeaponManager weaponManager;
    
    private float _currentIkWeight;
    private Vector3 _targetHandPosition;
    private Quaternion _targetHandRotation;
    private Quaternion _smoothedRotation;
    private bool _isReloading;
    private Vector3 _velocity;
    
        
    [SerializeField] private string gunTypeParam = "modelType";
    [SerializeField] private string powerChargeParam = "Charging";
    [SerializeField] private string powerTypeParam = "powerType";
    [SerializeField] private int mag7ID = 3;

    PlayerShotgunIk playerShotgunIk;
    
    private void Start()
    {
        playerShotgunIk = GetComponent<PlayerShotgunIk>();
    }


    private void OnAnimatorIK(int layerIndex)
    {
        // Ensure we are on the correct layer
        if (layerIndex != animator.GetLayerIndex("WallRunIk")) return;
        
        //check animator parmaeters

        int isGunParamOn = animator.GetInteger(gunTypeParam);
        bool isPowerOn = animator.GetBool(powerChargeParam);
        int powerType = animator.GetInteger(powerTypeParam);
        bool playerUsingPower = playerShotgunIk.isFiringPower; 
        
        
        //layerIndex = 2; // Ensure we are on the base layer
        if (!wallRunning.IsWallRunning || wallRunning.IsWallRunningRight || _isReloading || isGunParamOn == mag7ID || isPowerOn || playerUsingPower)
        {
            // Reset IK weight if not wall running
            _currentIkWeight = Mathf.Lerp(_currentIkWeight, 0, ikWeightTransitionSpeed * Time.deltaTime);
            ApplyHandIK(AvatarIKGoal.LeftHand, _currentIkWeight);
            return;
        }
        if (wallRunning.IsWallRunningLeft)
        {
            UpdateHandPosition();
            UpdateHandRotation();
            ApplyHandIK(AvatarIKGoal.LeftHand, _currentIkWeight);
        }
    }

    private void UpdateHandPosition()
    {
        Vector3 predictedPosition = wallRunning.ContactInfo.point + _velocity * positionPrediction;
        Vector3 wallNormal = wallRunning.ContactInfo.normal;
        
        _targetHandPosition = predictedPosition 
                            + Vector3.up * verticalOffset
                            + wallNormal * handPositionOffset
                            + Vector3.Cross(wallNormal, Vector3.up).normalized * forwardOffset;

        float distanceToShoulder = Vector3.Distance(
            animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position,
            _targetHandPosition
        );
        
        _currentIkWeight = Mathf.Lerp(_currentIkWeight, 
            Mathf.Clamp01(maxArmExtension / distanceToShoulder), 
            ikWeightTransitionSpeed * Time.deltaTime);
    }

    private void UpdateHandRotation()
    {
        Vector3 wallNormal = wallRunning.ContactInfo.normal;
        
        // Base rotation looking away from wall
        Quaternion baseRotation = Quaternion.LookRotation(-wallNormal);
        
        // Combined rotation offsets
        Quaternion combinedOffset = Quaternion.Euler(
            elbowRotationOffset + handRotationOffset
        );

        // Target rotation with smoothing
        _targetHandRotation = baseRotation * combinedOffset;
        _smoothedRotation = Quaternion.Slerp(
            _smoothedRotation, 
            _targetHandRotation, 
            1 - rotationSmoothing
        );
    }

    private void ApplyHandIK(AvatarIKGoal hand, float weight)
    {
        animator.SetIKPositionWeight(hand, weight);
        animator.SetIKRotationWeight(hand, weight);
        
        if (weight > 0.1f)
        {
            animator.SetIKPosition(hand, _targetHandPosition);
            animator.SetIKRotation(hand, _smoothedRotation);
        }
    }
    
    //on Reload Start stop ik
    public void OnReloadStart()
    {
        _isReloading = true;
    }
    
    public void OnReloadStop()
    {
        _isReloading = false;
    }
    
    

    private void Update() => _velocity = wallRunning.GetComponent<Rigidbody>().velocity; 

    // Add these to make tweaking easier in the editor
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !wallRunning.IsWallRunningLeft) return;
        
        // Draw hand position
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_targetHandPosition, 0.05f);
        
        // Draw rotation axes
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_targetHandPosition, _smoothedRotation * Vector3.right * 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_targetHandPosition, _smoothedRotation * Vector3.up * 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_targetHandPosition, _smoothedRotation * Vector3.forward * 0.2f);
    }
    #endif
}