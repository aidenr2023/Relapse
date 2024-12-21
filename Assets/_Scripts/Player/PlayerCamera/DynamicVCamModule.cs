using System;

[Serializable]
public abstract class DynamicVCamModule
{
    protected PlayerVirtualCameraController playerVCamController;

    public void Initialize(PlayerVirtualCameraController controller)
    {
        playerVCamController = controller;

        // Add the module to the player virtual camera controller
        controller.AddCameraModule(this);

        // Custom initialization
        CustomInitialize(controller);
    }

    protected abstract void CustomInitialize(PlayerVirtualCameraController controller);

    public abstract void Start();

    public abstract void Update();
}