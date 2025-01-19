using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ManagedAudioSource : MonoBehaviour
{
    #region Private Fields

    private AudioSource _audioSource;
    private Sound _sound;

    private bool _isPaused;

    private bool _isPermanent;

    #endregion

    #region Getters

    public AudioSource AudioSource => _audioSource;

    #endregion

    private void Awake()
    {
        // Get the audio source component
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // If the game is paused, pause the audio source
        // Otherwise, unpause the audio source
        if (MenuManager.Instance.IsGamePausedInMenus)
            Pause();
        else
            Resume();

        // If the audio source is done playing, destroy the game object
        if (!_audioSource.isPlaying && !_isPaused && !_isPermanent)
        {
            Destroy(gameObject);
            return;
        }

        // TODO: Change pitch with time scale
    }

    private void Pause()
    {
        // Return if already paused
        if (_isPaused)
            return;

        // Pause the audio source
        _audioSource.Pause();

        // Set the paused flag to true
        _isPaused = true;
    }

    private void Resume()
    {
        // Return if not paused
        if (!_isPaused)
            return;

        // Unpause the audio source
        _audioSource.UnPause();

        // Set the paused flag to false
        _isPaused = false;
    }

    public void Play(Sound sound)
    {
        // Return if the sound is null
        if (sound == null)
            return;

        // Set the sound
        _sound = sound;

        // Set the clip and volume
        _audioSource.clip = sound.Clip;
        _audioSource.volume = sound.Volume;
        _audioSource.pitch = sound.Pitch;

        // If the sound is persistent, set the priority to the max
        if (sound.IsPersistent)
            _audioSource.priority = 256;
        
        // If the sound is looping, set the loop flag to true
        _audioSource.loop = sound.IsLooping;

        // Play the sound
        // _audioSource.Play();
        _audioSource.PlayOneShot(sound.Clip);
    }

    public void Stop()
    {
        // Stop the audio source
        _audioSource.Stop();
        
        // Set the clip to null
        _audioSource.clip = null;
        
        // Set the is paused flag to false
        _isPaused = false;
    }

    public void Kill()
    {
        // Stop the audio source
        _audioSource.Stop();

        // Set the clip to null
        _audioSource.clip = null;

        // Set the is paused flag to false
        _isPaused = false;

        // Destroy the game object
        Destroy(gameObject);
    }

    public void SetPermanent(bool isPermanent)
    {
        _isPermanent = isPermanent;
    }
}