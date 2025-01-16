using System;
using UnityEngine;

[Serializable]
public class Sound
{
    #region Serialized Fields

    [SerializeField] private AudioClip clip;

    [SerializeField] private SoundType soundType = SoundType.PlayerSfx;

    [SerializeField] [Range(0, 1)] private float volume = 1;
    [SerializeField] private bool isPersistent;
    [SerializeField] private bool isLooping;
    [SerializeField, Range(.01f, 2f)] private float pitch = 1;

    #endregion

    #region Getters

    public AudioClip Clip => clip;

    public SoundType SoundType => soundType;


    public bool IsPersistent => isPersistent;

    public float Volume => volume;

    public bool IsLooping => isLooping;
    
    public float Pitch => pitch;

    #endregion

    public override string ToString()
    {
        if (clip == null)
            return "";

        return $"[Clip: {clip?.name}, Type: {soundType}]";
    }
}