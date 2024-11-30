using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBreakAnimation : MonoBehaviour
{
    public Animator targetAnimator; // Assign the Animator that controls the animation
    public string animationTriggerName; // The name of the trigger parameter in the Animator
    public AudioSource soundEffect; // Assign the AudioSource for the sound effect
    public ParticleSystem particleEffect; // Assign the ParticleSystem to activate

    private bool hasTriggered = false; // Ensures the actions happen only once

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // Prevent repeated triggering

        if (other.CompareTag("Player"))
        {
            hasTriggered = true; // Mark as triggered

            // Trigger the animation
            if (targetAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
            {
                targetAnimator.SetTrigger(animationTriggerName);
            }

            // Play the sound effect
            if (soundEffect != null)
            {
                soundEffect.Play();
            }

            // Activate the particle effect
            if (particleEffect != null)
            {
                particleEffect.Play();
            }
        }
    }
}