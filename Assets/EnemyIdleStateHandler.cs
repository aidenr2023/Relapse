using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleStateHandler : StateMachineBehaviour
{
    [SerializeField] private float _timeUntilAttack = 3f; // Time before transitioning to idle
    [SerializeField] private float _idleTime = 0; // Tracks time spent stationary

    private float attackAnim;
    //private Rigidbody _rb; // Reference to Rigidbody for player movement

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //_rb = animator.GetComponentInParent<Rigidbody>(); // Get Rigidbody from the player object
        
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // randomize the attack animation to play
        attackAnim = Random.Range(0, 2);
        Debug.Log($"Enemy Animation is: {attackAnim}");
        // Update Animator with current velocity
        Debug.Log($"Setting Enemy Attack parameter: {attackAnim}");
        
        //on attack trigger chose random attack animation
        
        
        // Reset idle time if moving, otherwise increment
        if ( )
        {
            _
        }
        else
        {
            
        }

        // Determine if idle animation should trigger
        if ()
        {
            animator.SetFloat("AttackTrigger", 0, 0.1f, Time.deltaTime); // Set velocity to 0 to trigger idle in the blend tree
            Debug.Log("Idle time exceeded threshold. Setting playerSpeed to 0 for idle animation.");
        }

        
    }
}
