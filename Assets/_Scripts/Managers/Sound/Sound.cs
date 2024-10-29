using System;
using UnityEngine;

[Serializable]
public class Sound
{
    #region Serialized Fields

    [SerializeField] private AudioClip clip;
    [SerializeField] private SoundSettings settings;
    [SerializeField] [Range(0, 1)] private float volume = 1;
    [SerializeField] private bool isPersistent;

    #endregion

    #region Getters

    public AudioClip Clip => clip;

    public SoundSettings Settings => settings;

    public bool IsPersistent => isPersistent;

    public float Volume => volume;

    #endregion

    public override string ToString()
    {
        if (clip == null || settings == null)
            return "";

        return $"[Clip: {clip?.name}, Settings: {settings?.name}]";
    }
}