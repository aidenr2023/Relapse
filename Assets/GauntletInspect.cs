using UnityEngine;

public class GauntletInspectController : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    // Flag to track if the animation is currently paused
    private bool isPaused = false;

    // Optional: Flag to ensure PauseAnimation is only called once per event
    private bool hasPaused = false;

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Error handling if Animator is not found
        if (animator == null)
        {
            Debug.LogError("GauntletInspectController: Animator component not found on " + gameObject.name);
        }
    }

    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Handles user input for pressing and releasing the "I" key.
    /// </summary>
    private void HandleInput()
    {
        // Detect when the "I" key is pressed down
        if (Input.GetKeyDown(KeyCode.I))
        {
            TriggerGauntletInspect();
        }

        // Detect when the "I" key is released
        if (Input.GetKeyUp(KeyCode.I))
        {
            if (isPaused)
            {
                ResumeAnimation();
            }
        }
    }

    /// <summary>
    /// Triggers the GauntletInspect animation.
    /// </summary>
    private void TriggerGauntletInspect()
    {
        if (animator != null)
        {
            animator.SetTrigger("PlayTrigger");
            Debug.Log("GauntletInspectController: PlayTrigger activated.");
        }
    }

    /// <summary>
    /// Called by the Animation Event to pause the animation.
    /// </summary>
    public void PauseAnimation()
    {
        // Ensure the Animator exists and the "I" key is being held
        if (animator != null && Input.GetKey(KeyCode.I))
        {
            if (!hasPaused)
            {
                animator.speed = 0f; // Pause the animation
                isPaused = true;
                hasPaused = true; // Prevent multiple pauses from multiple events
                Debug.Log("GauntletInspectController: Animation paused via PauseAnimation event.");
            }
        }
        else
        {
            Debug.Log("GauntletInspectController: PauseAnimation event triggered, but 'I' key is not held.");
        }
    }

    /// <summary>
    /// Resumes the animation from where it was paused.
    /// </summary>
    private void ResumeAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1f; // Resume the animation
            isPaused = false;
            hasPaused = false; // Reset for potential future pauses
            Debug.Log("GauntletInspectController: Animation resumed after releasing 'I' key.");
        }
    }
}
