using UnityEngine;

public class DynamicPostProcessPair : MonoBehaviour
{
    [SerializeField] private DynamicPostProcessVolume worldVolume;
    [SerializeField] private DynamicPostProcessVolume screenVolume;
    [SerializeField, Range(0, 1)] private float weight = 1;

    public float Weight => weight;
    
    public DynamicPostProcessVolume WorldVolume => worldVolume;
    public DynamicPostProcessVolume ScreenVolume => screenVolume;

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
        worldVolume.Volume.weight = weight;
        screenVolume.Volume.weight = weight;
    }
}