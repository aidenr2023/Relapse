using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerVirtualCameraController : ComponentScript<Player>
{
    #region Serialized Fields

    [SerializeField] private CameraManagerReference cameraManager;
    [field: SerializeField] public UserSettingsVariable CurrentUserSettings { get; private set; }
    
    [Header("Camera Modules")] [SerializeField]
    private DynamicFOVModule dynamicFOVModule;

    [SerializeField] private DynamicRotationModule dynamicRotationModule;

    [SerializeField] private DynamicOffsetModule dynamicOffsetModule;

    [SerializeField] private DynamicNoiseModule dynamicNoiseModule;

    #endregion

    #region Private Fields

    private readonly HashSet<DynamicVCamModule> _cameraModules = new();

    #endregion

    #region Getters

    public CinemachineVirtualCamera VirtualCamera => cameraManager.Value?.VirtualCamera;

    public DynamicFOVModule DynamicFOVModule => dynamicFOVModule;
    public DynamicRotationModule DynamicRotationModule => dynamicRotationModule;
    public DynamicOffsetModule DynamicOffsetModule => dynamicOffsetModule;

    public DynamicNoiseModule DynamicNoiseModule => dynamicNoiseModule;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Initialize the virtual camera modules
        InitializeVCamModules();
    }

    private void InitializeVCamModules()
    {
        // There is no need to manually add the modules to the list of modules.
        // They will be added automatically when they are created via the initializer.

        // Create the dynamic FOV module
        dynamicFOVModule.Initialize(this);

        // Create the dynamic rotation module
        dynamicRotationModule.Initialize(this);

        // Create the dynamic offset module
        dynamicOffsetModule.Initialize(this);

        // Create the dynamic noise module
        dynamicNoiseModule.Initialize(this);
    }

    private void Start()
    {
        // Start the start modules coroutine
        StartCoroutine(StartModulesCoroutine());
    }

    private IEnumerator StartModulesCoroutine()
    {
        // Wait until the camera manager is initialized
        yield return new WaitUntil(() => cameraManager?.Value != null);

        // Start the camera modules
        foreach (var module in _cameraModules)
            module.Start();
    }


    private void Update()
    {
        // Update the camera modules
        foreach (var module in _cameraModules)
            if (module.IsStarted)
                module.Update();
    }

    public void AddCameraModule(DynamicVCamModule module)
    {
        // Add the module to the list of modules
        _cameraModules.Add(module);
    }
}