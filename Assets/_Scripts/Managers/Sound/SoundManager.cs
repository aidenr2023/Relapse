using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private SoundPool testPool;

    #endregion

    #region Getters

    public AudioSource MusicSource => musicSource;

    public AudioSource SfxSource => sfxSource;

    #endregion

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.PlayerControls.Debug.ToggleDebug.performed += SoundTest;
    }

    private void SoundTest(InputAction.CallbackContext obj)
    {
        // Get a random sound from the test pool
        var sound = testPool.GetRandomSound();

        // Play the sound
        PlaySfx(sound);
    }

    private void SetSoundSetting(Sound sound, AudioSource source)
    {
        // Assign the sound clip to the source
        source.clip = sound.Clip;

        // Assign the sound settings to the source
        source.loop = sound.Settings.Loop;
        source.volume = sound.Settings.Volume;
        source.pitch = sound.Settings.Pitch;
        source.panStereo = sound.Settings.StereoPan;
        source.spatialBlend = sound.Settings.SpatialBlend;
        source.reverbZoneMix = sound.Settings.ReverbZoneMix;
    }

    public void PlaySound(Sound sound, AudioSource source, bool stopAudio)
    {
        // Force Stop the audio if it's already playing
        if (stopAudio)
            source.Stop();

        // Set the sound settings
        SetSoundSetting(sound, source);

        // Play the sound
        source.PlayOneShot(sound.Clip);
    }

    public void PlaySoundAtPoint(Sound sound, AudioSource source, bool stopAudio, Vector3 pos)
    {
        // Move the source to the position
        source.transform.position = pos;

        // Play the sound
        PlaySound(sound, source, stopAudio);
    }

    public void PlayMusic(Sound sound) => PlaySound(sound, musicSource, true);

    public void PlaySfx(Sound sound) => PlaySound(sound, sfxSource, false);

    public void PlaySfxAtPoint(Sound sound, Vector3 pos) => PlaySoundAtPoint(sound, sfxSource, false, pos);

}