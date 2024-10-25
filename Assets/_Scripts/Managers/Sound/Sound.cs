using System;
using UnityEngine;

[Serializable]
public class Sound
{
    #region Serialized Fields

    [SerializeField] private AudioClip clip;
    [SerializeField] private SoundSettings settings;
    [SerializeField] private bool isPersistent;

    #endregion

    #region Getters

    public AudioClip Clip => clip;

    public SoundSettings Settings => settings;

    public bool IsPersistent => isPersistent;

    #endregion

    public override string ToString()
    {
        return $"[Clip: {clip.name}, Settings: {settings.name}]";
    }
}