using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicPostProcessVolume : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile profile;

    [Header("Modules"), SerializeField] private DynamicVignetteModule vignetteModule;
    [SerializeField] private DynamicChromaticAberrationModule chromaticAberrationModule;
    [SerializeField] private DynamicLiftGammaGainModule liftGammaGainModule;
    [SerializeField] private DynamicLensDistortionModule lensDistortionModule;

    #endregion

    #region Private Fields

    private readonly HashSet<DynamicPostProcessingModule> _modules = new();

    #endregion

    #region Getters

    public Volume Volume => volume;

    public VolumeProfile Profile => profile;

    public DynamicVignetteModule VignetteModule => vignetteModule;

    public DynamicChromaticAberrationModule ChromaticAberrationModule => chromaticAberrationModule;

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

        // Initialize the dynamic chromatic aberration module
        chromaticAberrationModule.Initialize(this);

        // Initialize the dynamic lift gamma gain module
        liftGammaGainModule.Initialize(this);

        // Initialize the dynamic lens distortion module
        lensDistortionModule.Initialize(this);
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
    
    public void TransferTokens(DynamicPostProcessVolume otherVolume)
    {
        // If the other volume is null, return
        if (otherVolume == null)
            return;
        
        // If the other volume is the same as this volume, return
        if (otherVolume == this)
            return;
        
        var myModules = _modules.ToArray();
        var otherModules = otherVolume._modules.ToArray();

        // Transfer tokens from each module
        for (var i = 0; i < myModules.Length; i++)
            myModules[i].TransferTokens(otherModules[i]);
    }
}