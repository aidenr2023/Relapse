﻿using System;
using UnityEngine;

[Serializable]
public class Sound
{
    #region Serialized Fields

    [SerializeField] private AudioClip clip;

    [SerializeField] private SoundType soundType = SoundType.PlayerSfx;

    [SerializeField] [Range(0, 1)] private float volume = 1;
    [SerializeField] private bool isPersistent;

    #endregion

    #region Getters

    public AudioClip Clip => clip;

    public SoundType SoundType => soundType;


    public bool IsPersistent => isPersistent;

    public float Volume => volume;

    #endregion

    public override string ToString()
    {
        if (clip == null)
            return "";

        return $"[Clip: {clip?.name}, Type: {soundType}]";
    }
}