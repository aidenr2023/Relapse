using UnityEngine;

public class NPCBlendAnim : StateMachineBehaviour
{
    [SerializeField] private int numberOfIdleAnimations = 2; // Number of idle animations
    [SerializeField] private float timeUntilBored = 5f;      // Time until the NPC gets bored
    [SerializeField] private float timeBetweenIdleChanges = 3f; // Time between idle animation changes

    private float boredTimer = 0f;           // Timer for bored state
    private float idleAnimationTimer = 0f;   // Timer for idle animation changes
    private static readonly int IdleBlendHash = Animator.StringToHash("IdleBlend");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boredTimer = 0f;
        idleAnimationTimer = 0f;

        // Initialize IdleBlend parameter
       animator.SetFloat(IdleBlendHash, 0f);

       Debug.Log("Entered Idle State.");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Update timers
        boredTimer += Time.deltaTime;
        idleAnimationTimer += Time.deltaTime;

        // Check if it's time to change the idle animation
        if (idleAnimationTimer >= timeBetweenIdleChanges)
        {
            // Randomize the idle animation
            float randomBlendValue = Random.Range(0, numberOfIdleAnimations);
        
            // Smoothly transition to the new idle animation
            animator.SetFloat(IdleBlendHash, randomBlendValue, 0.1f, Time.deltaTime);
        
           // Debug.Log($"Idle animation changed with blend value: {randomBlendValue}");
        
            // Reset idle animation timer
            idleAnimationTimer = 0f;
        }

        // Check if the NPC should become bored
        if (boredTimer >= timeUntilBored)
        {
            animator.SetTrigger("Bored");
            // Debug.Log("NPC is bored.");

            // Reset bored timer
            boredTimer = 0f;
        }
    }
}
