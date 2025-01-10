using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private ManagedAudioSource musicSource;
    [SerializeField] private ManagedAudioSource playerSfxSource;
    [SerializeField] private ManagedAudioSource enemySfxSource;
    [SerializeField] private ManagedAudioSource otherSfxSource;
    [SerializeField] private ManagedAudioSource uiSfxSource;

    #endregion

    private void Awake()
    {
        // Set the instance
        Instance = this;

        // Initialize the audio sources
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        // Set the audio sources to be permanent
        musicSource.SetPermanent(true);
        playerSfxSource.SetPermanent(true);
        enemySfxSource.SetPermanent(true);
        otherSfxSource.SetPermanent(true);
        uiSfxSource.SetPermanent(true);
    }

    public ManagedAudioSource PlaySfxAtPoint(Sound sound, Vector3 position)
    {
        // If the sound is null, return
        if (sound == null)
            return null;

        // Based on the sound type, get the correct source
        var currentSource = sound.SoundType switch
        {
            SoundType.Music => musicSource,
            SoundType.PlayerSfx => playerSfxSource,
            SoundType.EnemySfx => enemySfxSource,
            SoundType.OtherSfx => otherSfxSource,
            SoundType.UiSfx => uiSfxSource,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (currentSource == null)
        {
            Debug.LogError($"{sound.SoundType} source is null!");
            return null;
        }

        // Instantiate the audio source at the given position
        var audioSource = Instantiate(currentSource, position, Quaternion.identity);

        // Play the sound
        audioSource.Play(sound);

        return audioSource;
    }

    public ManagedAudioSource PlaySfx(Sound sound)
    {
        var audioSource = PlaySfxAtPoint(sound, Vector3.zero);

        // Return if the source is null
        if (audioSource == null)
            return null;

        // Set the spatial blend to 0 if the sound is not 3D
        audioSource.AudioSource.spatialBlend = 0;

        return audioSource;
    }
}