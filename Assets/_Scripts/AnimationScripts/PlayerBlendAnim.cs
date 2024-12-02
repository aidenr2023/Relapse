using UnityEngine;

public class PlayerBlendAnim : StateMachineBehaviour
{
    [SerializeField] private float _timeUntilIdle = 3f; // Time before transitioning to idle
    [SerializeField] private float _idleTime = 0; // Tracks time spent stationary
    private Rigidbody _rb; // Reference to Rigidbody for player movement

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _rb = animator.GetComponentInParent<Rigidbody>(); // Get Rigidbody from the player object
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Calculate velocity based on Rigidbody's movement
        var playerSpeed = _rb.velocity.magnitude;
        // Debug.Log($"Player Speed: {playerSpeed}");

        // Update Animator with current velocity
        // Debug.Log($"Setting Animator Velocity parameter: {playerSpeed}");
        
        
        // Reset idle time if moving, otherwise increment
        if (playerSpeed > 0.1f)
        {
            _idleTime = 0f; // Reset idle time when moving
            animator.SetFloat("Movement", 1, 0.1f, Time.deltaTime); 
            // Debug.Log("Player is moving. Idle time reset to 0.");
        }
        else
        {
            _idleTime += Time.deltaTime; // Increment idle time when stationary
            // Debug.Log($"Player is stationary. Idle time incremented: {_idleTime}");
        }

        // Determine if idle animation should trigger
        if (_idleTime > _timeUntilIdle)
        {
            animator.SetFloat("Movement", 0, 0.1f, Time.deltaTime); // Set velocity to 0 to trigger idle in the blend tree
            // Debug.Log("Idle time exceeded threshold. Setting playerSpeed to 0 for idle animation.");
        }

        
    }
}