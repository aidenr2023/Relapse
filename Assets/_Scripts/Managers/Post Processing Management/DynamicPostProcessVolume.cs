using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class DynamicPostProcessVolume : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile profile;

    [Header("Modules"), SerializeField] private DynamicVignetteModule vignetteModule;

    #endregion

    #region Private Fields

    private readonly HashSet<DynamicPostProcessingModule> _modules = new();

    #endregion

    #region Getters

    public Volume Volume => volume;

    public VolumeProfile Profile => profile;

    public DynamicVignetteModule VignetteModule => vignetteModule;

    #endregion

    private void Awake()
    {
        // Initialize all modules
        InitializeModules();
    }

    private void InitializeModules()
    {
        // Initialize the dynamic vignette module
        vignetteModule.Initialize(this);
    }

    private void Start()
    {
        // Start all modules
        foreach (var module in _modules)
            module.Start();
    }

    private void Update()
    {
        // Update all modules
        foreach (var module in _modules)
            module.Update();
    }

    public void AddModule(DynamicPostProcessingModule module)
    {
        // Add the module to the post-processing volume controller
        _modules.Add(module);
    }

    public bool GetActualComponent<T>(out T component) where T : VolumeComponent
    {
        return volume.profile.TryGet(out component);
    }

    public bool GetSettingsComponent<T>(out T component) where T : VolumeComponent
    {
        return profile.TryGet(out component);
    }
}