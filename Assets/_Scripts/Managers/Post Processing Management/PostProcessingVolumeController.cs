using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessingVolumeController : MonoBehaviour
{
    public static PostProcessingVolumeController Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private DynamicPostProcessVolume screenVolume;
    [SerializeField] private DynamicPostProcessVolume worldVolume;

    #endregion

    #region Getters

    public DynamicPostProcessVolume ScreenVolume => screenVolume;
    public DynamicPostProcessVolume WorldVolume => worldVolume;

    public DynamicVignetteModule VignetteModule => screenVolume.VignetteModule;

    #endregion

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
    }
}