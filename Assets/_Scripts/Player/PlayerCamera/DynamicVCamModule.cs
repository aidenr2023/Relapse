using System;

[Serializable]
public abstract class DynamicVCamModule
{
    protected PlayerVirtualCameraController playerVCamController;

    public bool IsStarted { get; private set; }
    
    public void Initialize(PlayerVirtualCameraController controller)
    {
        playerVCamController = controller;

        // Add the module to the player virtual camera controller
        controller.AddCameraModule(this);

        // Custom initialization
        CustomInitialize(controller);
    }

    protected abstract void CustomInitialize(PlayerVirtualCameraController controller);

    public void Start()
    {
        // Set the module as started
        IsStarted = true;
        
        // Custom start
        CustomStart();
    }
    
    protected abstract void CustomStart();

    public abstract void Update();
}