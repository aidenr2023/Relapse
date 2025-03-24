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
        Play(_audioSource, sound, true);
    }

    public static void Play(AudioSource source, Sound sound, bool isPlayOneShot = false)
    {
        // Return if the source or sound is null
        if (source == null || sound == null)
            return;

        // Set the clip and volume
        source.clip = sound.Clip;
        source.volume = sound.Volume;
        source.pitch = sound.Pitch;
        source.spatialBlend = sound.SpatialBlend;

        // If the sound is persistent, set the priority to the max
        if (sound.IsPersistent)
            source.priority = 256;

        // If the sound is looping, set the loop flag to true
        source.loop = sound.IsLooping;

        // Play the sound
        if (!isPlayOneShot)
            source.Play();
        else
            source.PlayOneShot(sound.Clip);
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