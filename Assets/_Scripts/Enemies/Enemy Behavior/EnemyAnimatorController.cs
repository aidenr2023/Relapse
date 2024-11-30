using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : StateMachineBehaviour
{
    // Reference to the NavMeshAgent
    public NavMeshAgent navMeshAgent;

    // Reference to the Animator
    public Animator animator;

    // public bool canMove = false;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the EnemyAnimatorController component from the parent object
        
        
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyMovement();
        PickRandomState(animator, 3);
    }
    
    private void EnemyMovement()
    {
        // Get the current speed from the NavMeshAgent
        float speed = navMeshAgent.velocity.magnitude;

        // Set the Speed parameter in the Animator
        animator.SetFloat("Speed", speed);

        // Optionally, you can control other NPC behaviors or animations here as well
        animator.speed = navMeshAgent.speed / 3f;
    }
    
    
    
    //from substate machine pick random state
    public void PickRandomState(Animator animator, int numberOfStates)
    {
        if (numberOfStates <= 0)
        {
            Debug.LogWarning("Number of states must be greater than 0.");
            return;
        }

        // Pick a random number between 0 and (numberOfStates - 1)
        int randomState = Random.Range(0, numberOfStates);

        // Set the Animator parameter to the random state
        animator.SetInteger("RandomState", randomState);

        Debug.Log($"Picked Random State: {randomState}");
    }
    
}