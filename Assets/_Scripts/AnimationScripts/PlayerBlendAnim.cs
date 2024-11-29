using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlendAnim : StateMachineBehaviour
{
    // Parameters for controlling the blend tree
    [SerializeField] private float _timeUntilIdle;

    [SerializeField] float idleTime;
    [SerializeField]  int _numidleStates;
    
    [SerializeField] bool isMoving;
    
   
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResetIdle(animator);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isMoving == false)
        {
            idleTime += Time.deltaTime;
            if (idleTime > _timeUntilIdle)
            {
                isMoving = true;
                int randomIdle = Random.Range(0, _numidleStates+1);
                
                animator.SetFloat("idleAnimatins", randomIdle);
            }
        }
        else if(stateInfo.normalizedTime % 1 > 0.9f)
        {
            ResetIdle(animator);
        }
    }

    private void ResetIdle(Animator animator)
    {
        isMoving = false;
        idleTime = 0;
        animator.SetFloat("idleAnimatins", 0);
    }
    
}
