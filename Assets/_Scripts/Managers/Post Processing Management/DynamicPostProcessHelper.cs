using UnityEngine;

public class DynamicPostProcessHelper : MonoBehaviour
{
    [SerializeField] private PostProcessingType postProcessingType;
    [SerializeField, Min(0)] private float transitionTime = .5f;

    public void ChangePostProcessingType()
    {
        PostProcessingVolumeController.Instance.ChangePostProcessing(postProcessingType, transitionTime);
    }

}