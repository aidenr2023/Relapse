using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIk : MonoBehaviour
{
    [SerializeField]private Animator _animator;

    // IK target for the hand (where the hand should grip the pistol)
    [SerializeField] private Transform pistolGrip;  // Reference to the pistol grip point
    [SerializeField] private Transform rightHandIKTarget;  // Reference to the hand's IK target

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // OnAnimatorIK is called when the IK system is applied
    private void OnAnimatorIK(int layerIndex)
    {
        // Check if the character is armed (holding the pistol)
        if (_animator.GetBool("isArmed"))
        {
            // Set the IK position and rotation for the right hand
            _animator.SetIKPosition(AvatarIKGoal.RightHand, pistolGrip.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, pistolGrip.rotation);

            // Optional: Set the weight of IK to control the influence of IK on the animation
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        }
        else
        {
            // If the character is not armed, reset IK to prevent any undesired positions
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }
}
