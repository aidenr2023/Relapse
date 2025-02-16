using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// ScriptableObject that holds cutscene data, including the timeline asset and a virtual camera reference.
/// </summary>
[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    // Timeline asset for the cutscene.
    public PlayableAsset timelineAsset;
    
    // Reference to the Cinemachine virtual camera for the cutscene.
    public CinemachineVirtualCameraBase virtualCamera;
}