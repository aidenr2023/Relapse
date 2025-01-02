using UnityEngine;
using System.Collections;

public class TriggerPlayTwoAnimators : MonoBehaviour
{
    [Header("First Animation Settings")]
    public Animator firstAnimator;       // Assign in Inspector
    public string firstAnimationName;    // The state name in the first Animator

    [Header("Second Animation Settings")]
    public Animator secondAnimator;      // Assign in Inspector
    public string secondAnimationName;   // The state name in the second Animator

    [Header("Timing")]
    public float delayBetweenAnimations = 1f;  // The delay after the first animation finishes

    private bool hasTriggered = false;   // Prevent multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if it's the Player and hasn't triggered yet
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(PlayAnimationsSequentially());
        }
    }

    private IEnumerator PlayAnimationsSequentially()
    {
        // --- 1) Play first animation (on first Animator) ---
        if (firstAnimator && !string.IsNullOrEmpty(firstAnimationName))
        {
            firstAnimator.Play(firstAnimationName);

            // Wait for the first animation to finish
            float firstAnimLength = firstAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(firstAnimLength);
        }

        // --- 2) Wait the specified delay ---
        yield return new WaitForSeconds(delayBetweenAnimations);

        // --- 3) Play second animation (on second Animator) ---
        if (secondAnimator && !string.IsNullOrEmpty(secondAnimationName))
        {
            secondAnimator.Play(secondAnimationName);

            // (Optional) Wait for the second animation to finish
            // float secondAnimLength = secondAnimator.GetCurrentAnimatorStateInfo(0).length;
            // yield return new WaitForSeconds(secondAnimLength);
        }
    }
}
