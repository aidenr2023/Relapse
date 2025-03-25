using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHelper : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Get the AudioSource component
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(int index)
    {
        if (sounds == null || index < 0 || index >= sounds.Length)
        {
            Debug.LogError("Invalid sound index");
            return;
        }

        // Set the audio clip to the sound at the given index
        ManagedAudioSource.Play(_audioSource, sounds[index], false);
    }
    
    public void StopAudio()
    {
        // Stop the audio source
        _audioSource.Stop();
    }
}