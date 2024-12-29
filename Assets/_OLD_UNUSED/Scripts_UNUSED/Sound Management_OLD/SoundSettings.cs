using UnityEngine;

[CreateAssetMenu(fileName = "SoundSettings", menuName = "Sound Settings", order = 0)]
public class SoundSettings : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private bool loop = false;
    [SerializeField] [Range(-3, 3)] private float pitch = 1f;
    [SerializeField] [Range(-1, 1)] private float stereoPan = 0f;
    [SerializeField] [Range(0, 1)] private float spatialBlend = .95f;
    [SerializeField] [Range(0, 1.1f)] private float reverbZoneMix = 1f;

    #endregion

    #region Getters

    public bool Loop => loop;
    public float Pitch => pitch;
    public float StereoPan => stereoPan;
    public float SpatialBlend => spatialBlend;
    public float ReverbZoneMix => reverbZoneMix;

    #endregion
}