using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EnemyIdleStateHandler : StateMachineBehaviour
{
    [SerializeField] private float numberOfAttackAnimations = 2f; // Time before transitioning to idle
    private static readonly int animatiorAttackProperty = Animator.StringToHash("Attack");
    private float attackAnim;
    
    int numberofAttackAnimations = 3;
    private static readonly int AttackBlendHash = Animator.StringToHash("AttackBlend");
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
    [SerializeField] private Animator animator;

    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    public void TriggerAttack()
    {
        animator.SetTrigger(AttackTriggerHash);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Randomize the attack blend value
        float randomBlendValue = Random.Range(0, numberOfAttackAnimations);

        // Set the blend parameter dynamically
        animator.SetFloat(AttackBlendHash, randomBlendValue);

        Debug.Log($"Random attack animation selected: {randomBlendValue}");
   
    }
}
