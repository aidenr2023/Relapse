using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EnemyIdleStateHandler : StateMachineBehaviour
{
    [SerializeField] private int numberOfAttackAnimations = 2; // Number of attack animations
    private static readonly int AnimatorAttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackBlendHash = Animator.StringToHash("AttackBlend");
    private bool hasTriggeredAttack = false; // Prevent spamming attacks
    void UpdateStateInfo(Animator animator)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Lesion_Attack"))
        {
            Debug.Log("The Animator is now in the Attack State.");
            hasTriggeredAttack = true;
        }
    }
    // Called when the state is entered
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset the attack flag
        hasTriggeredAttack = false;
        Debug.Log("Entered Idle State. Waiting to attack...");
    }

    // Called on each update frame
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Update state info to check on attack state
        UpdateStateInfo(animator);
        if (!hasTriggeredAttack) // Ensure the attack triggers only once
        {
            // Trigger the attack
            //animator.SetTrigger(AnimatorAttackTrigger);

            // Randomize the attack animation
            float randomBlendValue = Random.Range(0, numberOfAttackAnimations);
            animator.SetFloat(AttackBlendHash, randomBlendValue);

            Debug.Log($"Attack was triggered in prev state with blend value: {randomBlendValue}");

            // Set the flag to prevent repeated triggering
            hasTriggeredAttack = true;
        }
    }
}
