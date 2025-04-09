using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShotgunIK : MonoBehaviour
{
    [Header("IK Targets")]
    public Transform sliderTransform;
    
    [Header("Model Configuration")]
    [SerializeField] private GunModelType modelType = GunModelType.Mag7;
    
    [Header("Animation Weights")]
    [SerializeField][Range(0,1)] private float leftHandIKWeight; // Controlled via animation curve
    
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
        
        // Optional: Auto-get reference if not set
        if (!sliderTransform)
            Debug.LogWarning("Slider Transform not assigned in Inspector!", this);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!_animator) return;

        // Only enable IK for Mag7 model type
        bool useIK = modelType == GunModelType.Mag7;
        float effectiveWeight = useIK ? leftHandIKWeight : 1f;

        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, effectiveWeight);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, effectiveWeight);

        if (useIK && sliderTransform)
        {
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, sliderTransform.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, sliderTransform.rotation);
        }
    }
}

