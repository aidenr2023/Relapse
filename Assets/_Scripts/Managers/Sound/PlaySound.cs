using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    // The AudioSource to play the sound from
    public AudioSource audioSource;

    // A multiplier to adjust the volume of the audio
    [Range(0f, 3f)] public float volumeMultiplier = 1f; // You can now set values above 1 (up to 3 or beyond)

    // A flag to ensure the sound is only played once
    private bool hasPlayed = false;

    // When the player enters the trigger volume
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player") && !hasPlayed)
        {
            // Set the volume based on the multiplier
            audioSource.volume = Mathf.Clamp(volumeMultiplier, 0f, 10f); // You can set a higher max limit here if needed

            // Play the audio
            audioSource.Play();

            // Set the flag so the audio doesn't play again
            hasPlayed = true;
        }
    }
}