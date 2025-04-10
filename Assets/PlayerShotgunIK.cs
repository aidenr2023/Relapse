using UnityEngine;

public class PlayerShotgunIk : MonoBehaviour
{
    [Header("Hand Targets")]
    [SerializeField] private Transform leftHandTarget;
    //[SerializeField] private Transform rightHandTarget;

    [Header("Animator Parameters")]
    [SerializeField] private string gunTypeParam = "modelType";
    [SerializeField] private string powerChargeParam = "Charging";
    [SerializeField] private int mag7ID = 1;

    [Header("IK Settings")]
    [SerializeField] private float ikActivationSpeed = 5f; // Smooth transition


    public bool isFiringPower = false;

    public void OnPowerFireStart()
    {
        isFiringPower = true;
    }

    public void OnPowerFireEnd()
    {
        isFiringPower = false;
    }
    
    
    private Animator _animator;
    private float _currentLeftWeight;
    //private float _currentRightWeight;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get target weights
        int currentGunType = _animator.GetInteger(gunTypeParam);
        float targetWeight = (currentGunType == mag7ID) ? 1f : 0f;

        // Smoothly transition weights
        _currentLeftWeight = Mathf.Lerp(_currentLeftWeight, targetWeight, 
            Time.deltaTime * ikActivationSpeed);
        // _currentRightWeight = Mathf.Lerp(_currentRightWeight, targetWeight, 
        //     Time.deltaTime * ikActivationSpeed);
    }

    void OnAnimatorIK(int layerIndex)
    {
        // Ensure we are on the correct layer
        if (layerIndex != _animator.GetLayerIndex("ShotgunIk")) return;
        
        // Check if animator is assigned
        if (!_animator) return;
        
        //checks to see if ik can be allowed
        if(_animator.GetBool(powerChargeParam) || isFiringPower) return;

        // Apply weights directly
        SetHandIK(AvatarIKGoal.LeftHand, leftHandTarget, _currentLeftWeight);
      //  SetHandIK(AvatarIKGoal.RightHand, rightHandTarget, _currentRightWeight);
    }

    void SetHandIK(AvatarIKGoal hand, Transform target, float weight)
    {
        _animator.SetIKPositionWeight(hand, weight);
        _animator.SetIKRotationWeight(hand, weight);
        
        if (weight > 0.01f && target != null)
        {
            _animator.SetIKPosition(hand, target.position);
            _animator.SetIKRotation(hand, target.rotation);
        }
    }
}