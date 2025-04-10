using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DynamicMotionBlurModule : DynamicPostProcessingModule
{
    protected override void CustomInitialize(DynamicPostProcessVolume controller)
    {
    }

    public override void Start()
    {
    }

    public override void Update()
    {
        // Get the actual motion blur component
        dynamicVolume.GetActualComponent(out MotionBlur actualMotionBlur);
        
        // Return if the screen motion blur is null
        if (actualMotionBlur == null)
            return;
        
        // Get the motion blur settings
        dynamicVolume.GetActualComponent(out actualMotionBlur);
        
        // Set the motion blur settings
        actualMotionBlur.intensity.value = dynamicVolume.UserSettings.value.MotionBlur;
    }

    public override void TransferTokens(DynamicPostProcessingModule otherModule)
    {
    }
}