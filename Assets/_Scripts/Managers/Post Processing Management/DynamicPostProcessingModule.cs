public abstract class DynamicPostProcessingModule
{
    protected DynamicPostProcessVolume dynamicVolume;

    public void Initialize(DynamicPostProcessVolume controller)
    {
        this.dynamicVolume = controller;

        // Add the module to the post-processing volume controller
        controller.AddModule(this);

        // Custom initialization
        CustomInitialize(controller);
    }

    protected abstract void CustomInitialize(DynamicPostProcessVolume controller);

    public abstract void Start();

    public abstract void Update();
    
    public abstract void TransferTokens(DynamicPostProcessingModule otherModule);
}