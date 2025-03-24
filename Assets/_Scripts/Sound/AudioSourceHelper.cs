using System;
using UnityEngine;

public class AudioSourceHelper : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        // Get the AudioSource component
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PlayAudio()
    {
        // Play the audio
        _audioSource.Play();
    }
}