using System;
using UnityEngine;

[Serializable]
public class Sound
{
    #region Serialized Fields

    [SerializeField] private AudioClip clip;
    [SerializeField] private SoundSettings settings;

    #endregion

    #region Getters

    public AudioClip Clip => clip;

    public SoundSettings Settings => settings;

    #endregion
}